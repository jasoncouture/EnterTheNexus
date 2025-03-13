using Microsoft.Extensions.DependencyInjection;

namespace EnterTheNexus.Network.Abstractions;

public class PacketContext<T> : IPacketContext<T> where T : class
{
    public PacketContext(T packet, IServerClient<T> serverClient)
    {
        Packet = packet;
        ServerClient = serverClient;
        Properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }

    public T Packet { get; }
    public IServerClient<T> ServerClient { get; }
    public IDictionary<string, object> Properties { get; }

    public IPacketHandler<TPacket> CreateHandler<TPacket>(IServiceProvider serviceProvider) where TPacket : class
    {
        return (IPacketHandler<TPacket>)serviceProvider.GetRequiredService<IPacketHandler<T>>();
    }
}