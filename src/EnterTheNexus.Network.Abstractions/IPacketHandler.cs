namespace EnterTheNexus.Network.Abstractions;

public interface IPacketHandler<in T> where T : class
{
    ValueTask HandleAsync(T packet, CancellationToken cancellationToken);
}