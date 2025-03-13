namespace EnterTheNexus.Network.Abstractions;

public interface IPacketContext<out TPacket> where TPacket : class
{
    TPacket Packet { get; }
    public IServerClient<TPacket> ServerClient { get; }
    public IDictionary<string, object> Properties { get; }
    public IPacketHandler<T> CreateHandler<T>(IServiceProvider serviceProvider) where T : class;
}