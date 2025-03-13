using System.Net.Sockets;

namespace EnterTheNexus.Network.Abstractions;

public interface IServerClient<out TPacket> where TPacket : class
{
    ICommunicationChannel CommunicationChannel { get; }
    bool IsConnected { get; }
    Task RunAsync(CancellationToken stoppingToken);
}