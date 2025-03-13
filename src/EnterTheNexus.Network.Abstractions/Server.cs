using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace EnterTheNexus.Network.Abstractions;

public sealed class Server<TPacket> : IServer<TPacket> where TPacket : class
{
    private readonly IPEndPoint _endPoint;
    private readonly IServerClientFactory<TPacket> _clientFactory;

    [SuppressMessage("ReSharper", "CollectionNeverQueried.Local",
        Justification = "Used to prevent garbage collection of clients")]
    private readonly HashSet<IServerClient<TPacket>> _clients = new HashSet<IServerClient<TPacket>>();

    private readonly IServiceProvider _serviceProvider;

    public Server(IPEndPoint endPoint, IServerClientFactory<TPacket> clientFactory, IServiceProvider serviceProvider)
    {
        _endPoint = endPoint;
        _clientFactory = clientFactory;
        _serviceProvider = serviceProvider;
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        using var clientShutdownToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        try
        {
            using var listener = new TcpListener(_endPoint);
            listener.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(stoppingToken);
                using var _ = ExecutionContext.SuppressFlow();
                await OnClientAcceptedAsync(client, clientShutdownToken.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        finally
        {
            await clientShutdownToken.CancelAsync();
            lock (_clients)
            {
                _clients.Clear();
            }
        }
    }

    public void ClientDisconnected(IServerClient<TPacket> client)
    {
        lock (_clients)
        {
            _clients.Remove(client);
        }
    }

    private async Task OnClientAcceptedAsync(TcpClient client, CancellationToken stoppingToken)
    {
        var newClient = _clientFactory.Create(client, this, _serviceProvider, stoppingToken);
        newClient.RunAsync(stoppingToken).Orphan();
        if (!newClient.IsConnected) return;
        lock (_clients)
        {
            if (!newClient.IsConnected) return;
            _clients.Add(newClient);
        }

    }
}