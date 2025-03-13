namespace EnterTheNexus.Network.WildStarGameProtocol.Authentication;

public class HardwareInformation : IWildStarPacket
{
    public required CpuInformation Cpu { get; init; }
    public uint MemoryPhysical { get; init; }
    public required GpuInformation Gpu { get; init; }
    public uint Architecture { get; init; }
    public uint OsVersion { get; init; }
    public uint ServicePack { get; init; }
    public uint ProductType { get; init; }
}