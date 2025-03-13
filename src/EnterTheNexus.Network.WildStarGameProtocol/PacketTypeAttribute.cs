namespace EnterTheNexus.Network.WildStarGameProtocol;

[AttributeUsage(AttributeTargets.Class)]
public class PacketTypeAttribute : Attribute
{
    public PacketTypeAttribute(PacketType type) => Type = type;
    public PacketType Type { get; }
}