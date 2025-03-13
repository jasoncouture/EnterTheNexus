namespace EnterTheNexus.Network.Abstractions;

public interface IPacketContextAccessor<out TPacket> where TPacket : class
{
    public IPacketContext<TPacket>? PacketContext { get; }
}