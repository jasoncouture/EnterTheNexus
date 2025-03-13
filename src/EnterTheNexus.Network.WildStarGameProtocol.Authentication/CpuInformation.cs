namespace EnterTheNexus.Network.WildStarGameProtocol.Authentication;

public class CpuInformation : IWildStarPacket
{
    public required string Manufacturer { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public int Family { get; init; }
    public int Level { get; init; }
    public int Revision { get; init; }
    public int MaxClockSpeed { get; init; }
    public int NumberOfCores { get; init; }
}