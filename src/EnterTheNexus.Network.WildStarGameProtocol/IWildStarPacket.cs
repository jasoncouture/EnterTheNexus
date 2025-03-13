namespace EnterTheNexus.Network.WildStarGameProtocol;

public interface IWildStarPacket
{
    public bool CanSerializeSelf => false;
    public bool CanDeserializeSelf => false;
    public int GetEstimatedPacketLength() => throw new NotImplementedException();
    public int Serialize(Memory<byte> memory) => throw new NotImplementedException();
    public void Deserialize(BitReader bitReader) => throw new NotImplementedException();
}