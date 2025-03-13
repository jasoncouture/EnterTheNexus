using System.Buffers;

namespace EnterTheNexus.Network.WildStarGameProtocol;

public class BitPosition
{
    public int BitOffset { get; private set; }
    public int ByteOffset { get; private set; }

    public BitPosition Reset()
    {
        BitOffset = 0;
        ByteOffset = 0;
        return this;
    }

    public void AdvanceBits(int count)
    {
        var bits = (BitOffset + count) % 8;
        var bytes = (BitOffset + count) / 8;
        BitOffset = bits;
        ByteOffset += bytes;
    }

    public void AdvanceBytes(int count, bool retainBitPosition = false)
    {
        if (!retainBitPosition)
        {
            BitOffset = 0;
        }

        ByteOffset++;
    }

    public ReadOnlySequence<byte> GetSlice(ReadOnlySequence<byte> buffer)
    {
        return buffer.Slice(ByteOffset);
    }
}