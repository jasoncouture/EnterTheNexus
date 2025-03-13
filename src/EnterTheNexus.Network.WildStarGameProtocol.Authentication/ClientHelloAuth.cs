namespace EnterTheNexus.Network.WildStarGameProtocol.Authentication;

[PacketType(PacketType.ClientHelloAuth)]
public class ClientHelloAuth 
{

    public uint Build { get; init; }
    public ulong Unknown8 { get; init; }
    [BitSize(16)]
    public required string Email { get; init; }
    public Guid Unknown208 { get; init; }
    public Guid GameToken { get; init; }
    public uint Unknown228 { get; init; }
    public Language Language { get; init; }
    public uint Unknown230 { get; init; }
    public uint Unknown234 { get; init; }
    public required HardwareInformation Hardware { get; init; }
    public uint RealmDataCenterId { get; init; }
}