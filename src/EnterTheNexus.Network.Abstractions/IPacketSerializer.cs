using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace EnterTheNexus.Network.Abstractions;

public interface IPacketSerializer<TPacket>
{
    public int GetEstimatedPacketLength(TPacket packet);
    public int Serialize(TPacket packet, Memory<byte> memory);
    public bool TryDeserialize(ReadOnlySequence<byte> buffer, [NotNullWhen(true)] out TPacket? packet, out int length);
}