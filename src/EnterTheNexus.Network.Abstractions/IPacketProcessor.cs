using System.Buffers;

namespace EnterTheNexus.Network.Abstractions;

public interface IPacketProcessor<in TPacket> where TPacket : class
{
    ValueTask<PacketProcessingResult> ProcessPacketAsync(
        ReadOnlySequence<byte> buffer,
        IServerClient<TPacket> client,
        CancellationToken cancellationToken
    );
}