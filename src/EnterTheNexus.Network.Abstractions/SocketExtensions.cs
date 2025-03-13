using System.Buffers;
using System.Net.Sockets;

namespace EnterTheNexus.Network.Abstractions;

public static class SocketExtensions
{
    public static async ValueTask SendPacketAsync<TPacket>(this Socket socket, TPacket packet, IPacketSerializer<TPacket> serializer,
        CancellationToken cancellationToken) where TPacket : class
    {
        var length = serializer.GetEstimatedPacketLength(packet);
        var buffer = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            var actualLength = serializer.Serialize(packet, buffer);
            await socket.SendAsync(buffer.AsMemory()[..actualLength], SocketFlags.None, cancellationToken);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}