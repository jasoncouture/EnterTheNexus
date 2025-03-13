using System.Net.Sockets;

namespace EnterTheNexus.Network.Abstractions;

public interface IServerClientFactory<TPacket> where TPacket : class
{
    IServerClient<TPacket> Create(TcpClient client, IServer<TPacket> server, IServiceProvider serviceProvider,
        CancellationToken stoppingToken);
}