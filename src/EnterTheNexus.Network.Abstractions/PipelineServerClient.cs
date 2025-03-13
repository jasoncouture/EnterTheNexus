using System.IO.Pipelines;
using Microsoft.Extensions.DependencyInjection;

namespace EnterTheNexus.Network.Abstractions;

public sealed class PipelineServerClient<TPacket> : IServerClient<TPacket> where TPacket : class
{
    private readonly IServer<TPacket> _server;
    private readonly IPipeFactory<TPacket> _pipeFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;


    public ICommunicationChannel CommunicationChannel { get; }

    public PipelineServerClient(ICommunicationChannel communicationChannel, IServer<TPacket> server, IPipeFactory<TPacket> pipeFactory, IServiceScopeFactory serviceScopeFactory)
    {
        CommunicationChannel = communicationChannel;
        _server = server;
        _serviceScopeFactory = serviceScopeFactory;
        _pipeFactory = pipeFactory;
    }

    private Pipe CreatePipe()
    {
        return _pipeFactory.CreatePipe();
    }

    private async Task<int?> ProcessPacketAsync(ReadResult readResult, PipeReader pipeReader,
        CancellationToken stoppingToken)
    {
        await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
        var processor = serviceScope.ServiceProvider.GetRequiredService<IPacketProcessor<TPacket>>();
        var processingResult = await processor.ProcessPacketAsync(readResult.Buffer, this, stoppingToken);
        if (processingResult.Processed)
        {
            pipeReader.AdvanceTo(readResult.Buffer.GetPosition(processingResult.ConsumedLength));
            return null;
        }

        pipeReader.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);

        if (processingResult.ExpectedLength == 0)
        {
            return null;
        }

        return processingResult.ExpectedLength;
    }

    private async Task RunInboundPipeReaderAsync(PipeReader pipeReader,
        CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested && IsConnected)
            {
                var next = await pipeReader.ReadAsync(stoppingToken);
                var neededBytes = await ProcessPacketAsync(next, pipeReader, stoppingToken);
                if (neededBytes == null) continue;
                next = await pipeReader.ReadAtLeastAsync(neededBytes.Value, stoppingToken);
                await ProcessPacketAsync(next, pipeReader, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            // Ignored
        }
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var inboundPipe = CreatePipe();
        var inboundReader = inboundPipe.Reader;
        var inboundWriter = inboundPipe.Writer;
        var inboundPipeReaderTask = RunInboundPipeReaderAsync(inboundReader, stoppingToken);
        var inboundPipeWriterTask = RunInboundPipeWriterAsync(inboundWriter, stoppingToken);

        await Task.WhenAll(inboundPipeReaderTask, inboundPipeWriterTask)
            .WaitAsync(stoppingToken)
            .ConfigureAwait(false);
    }

    private async Task RunInboundPipeWriterAsync(PipeWriter pipeWriter, CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested && IsConnected)
            {
                var buffer = pipeWriter.GetMemory();
                var bytesRead = await CommunicationChannel.ReceiveAsync(buffer, stoppingToken);
                if (bytesRead == 0)
                {
                    return;
                }

                pipeWriter.Advance(bytesRead);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            await CompleteWriterAsync(pipeWriter, ex);
        }
        finally
        {
            await CompleteWriterAsync(pipeWriter);
        }
    }

    private async Task CompleteWriterAsync(PipeWriter pipeWriter, Exception? ex = null)
    {
        OnDisconnected();
        try
        {
            await pipeWriter.CompleteAsync(ex);
        }
        catch
        {
            // ignored
        }

        if (!IsConnected)
        {
            return;
        }

        try
        {
            CommunicationChannel.Close();
        }
        catch
        {
            // Ignored.
        }
    }

    public bool IsConnected => CommunicationChannel.IsConnected;

    private void OnDisconnected()
    {
        _server.ClientDisconnected(this);
    }
}