using System.Runtime.Serialization;
using EnterTheNexus.Network.WildStarGameProtocol;

namespace EnterTheNexus.Network.UnitTests;

[PacketType(PacketType.ClientCheat)]
public class TestPacket : IWildStarPacket
{

    public int Field1 { get; set; }
    [DataMember(Order = 2)]
    public int Field2 { get; set; }
    [DataMember(Order = 1)]
    public int Field3 { get; set; }
    [IgnoreDataMember] public string IgnoredField { get; set; } = "Ignored";
}