using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;

namespace EnterTheNexus.Network.Abstractions;

public abstract class Server<TPacket> : IServer<TPacket> where TPacket : class
{
    private readonly IServerClientFactory<TPacket> _clientFactory;

    [SuppressMessage("ReSharper", "CollectionNeverQueried.Local",
        Justification = "Used to prevent garbage collection of clients")]
    private readonly HashSet<IServerClient<TPacket>> _clients = new HashSet<IServerClient<TPacket>>();
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<ServerOptions> _optionsMonitor;

    public Server(IOptionsMonitor<ServerOptions> optionsMonitor, IServerClientFactory<TPacket> clientFactory, IServiceProvider serviceProvider)
    {
        _optionsMonitor = optionsMonitor;
        _clientFactory = clientFactory;
        _serviceProvider = serviceProvider;
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        using var clientShutdownToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        try
        {
            var endPoint = GetIPEndPoint();
            using var listener = new TcpListener(endPoint);
            listener.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(stoppingToken);
                using var _ = ExecutionContext.SuppressFlow();
                OnClientAccepted(client, clientShutdownToken.Token);
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

    private const int DefaultPort = 24000;

    private IPEndPoint GetIPEndPoint()
    {
        var uri = _optionsMonitor.CurrentValue.ListenUri;
        var host = uri.DnsSafeHost;
        var port = uri.Port;
        var scheme = uri.Scheme;
        if (scheme != "tcp")
        {
            throw new NotSupportedException($"Invalid listen URI '{uri}' unsupported scheme '{scheme}'");
        }

        if (port == 0)
        {
            throw new NotSupportedException($"Invalid listen URI '{uri}' port must be specified");
        }

        if (!IPAddress.TryParse(host, out var ipAddress))
        {
            throw new NotSupportedException($"Invalid listen URI '{uri}' host '{host}' must be an IP Address");
        }
        
        return new IPEndPoint(ipAddress, port);
    }

    public void ClientDisconnected(IServerClient<TPacket> client)
    {
        lock (_clients)
        {
            _clients.Remove(client);
        }
    }

    private void OnClientAccepted(TcpClient client, CancellationToken stoppingToken)
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