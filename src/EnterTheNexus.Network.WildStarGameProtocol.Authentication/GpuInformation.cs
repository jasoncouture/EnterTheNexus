namespace EnterTheNexus.Network.WildStarGameProtocol.Authentication;

public class GpuInformation : IWildStarPacket
{
    public required string Name { get; init; }
    public uint VendorId { get; init; }
    public uint DeviceId { get; init; }
    public uint SubSysId { get; init; }
    public uint Revision { get; init; }
    public uint Unknown10 { get; init; }
}