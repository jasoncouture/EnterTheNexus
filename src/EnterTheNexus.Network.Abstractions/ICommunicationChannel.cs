namespace EnterTheNexus.Network.Abstractions;

public interface ICommunicationChannel
{
    bool IsConnected { get; }
    ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken);
    ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);
    void Close();
}