namespace EnterTheNexus.Network.Abstractions;

public interface IServer<TPacket> where TPacket : class
{
    Task RunAsync(CancellationToken stoppingToken);
    void ClientDisconnected(IServerClient<TPacket> client);
}