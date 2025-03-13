using System.Buffers;
using Microsoft.Extensions.DependencyInjection;

namespace EnterTheNexus.Network.Abstractions;

public sealed class PacketProcessor<TPacket> : IPacketProcessor<TPacket> where TPacket : class, IPacketContext<TPacket>
{
    private readonly IPacketSerializer<TPacket> _serializer;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PacketProcessor(
        IPacketSerializer<TPacket> serializer,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _serializer = serializer;
        _serviceScopeFactory = serviceScopeFactory;
    }


    public async ValueTask<PacketProcessingResult> ProcessPacketAsync(ReadOnlySequence<byte> buffer,
        IServerClient<TPacket> client, CancellationToken cancellationToken)
    {
        if (!_serializer.TryDeserialize(buffer, out var packet, out var packetLength))
        {
            return PacketProcessingResult.IncompletePacket(packetLength);
        }

        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        var context = PacketContextFactory.Create<TPacket>(packet, client);
        var contextAccessor = serviceProvider.GetRequiredService<PacketContextAccessor<TPacket>>();
        contextAccessor.PacketContext = context;
        var runner = context.CreateHandler<TPacket>(serviceProvider);
        await runner.HandleAsync(packet, cancellationToken);
        return PacketProcessingResult.Completed(packetLength);
    }
}