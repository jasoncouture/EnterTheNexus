namespace EnterTheNexus.Network.Abstractions;

public class PacketContextAccessor<T> : IPacketContextAccessor<T> where T : class
{
    public IPacketContext<T>? PacketContext { get; set; }
}