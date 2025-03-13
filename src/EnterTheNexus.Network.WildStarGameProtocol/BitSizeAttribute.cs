namespace EnterTheNexus.Network.WildStarGameProtocol;

[AttributeUsage(AttributeTargets.Property)]
public class BitSizeAttribute(int bits) : Attribute
{
    public int Bits { get; } = bits;
}