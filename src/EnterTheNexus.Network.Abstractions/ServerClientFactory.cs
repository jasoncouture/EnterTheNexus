using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace EnterTheNexus.Network.Abstractions;

public class ServerClientFactory<TPacket> : IServerClientFactory<TPacket> where TPacket : class
{
    public IServerClient<TPacket> Create(TcpClient client, IServer<TPacket> server, IServiceProvider serviceProvider, CancellationToken stoppingToken)
    {
        return ActivatorUtilities.CreateInstance<IServerClient<TPacket>>(serviceProvider, client, server, stoppingToken);
    }
}