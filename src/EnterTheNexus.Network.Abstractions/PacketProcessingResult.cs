namespace EnterTheNexus.Network.Abstractions;

public record struct PacketProcessingResult(bool Processed, int ExpectedLength, int ConsumedLength)
{
    public static PacketProcessingResult IncompletePacket(int neededBytesTotal)
    {
        return new PacketProcessingResult(false, neededBytesTotal, 0);
    }

    public static PacketProcessingResult Completed(int consumedBytesTotal)
    {
        return new PacketProcessingResult(true, consumedBytesTotal, consumedBytesTotal);
    }
}