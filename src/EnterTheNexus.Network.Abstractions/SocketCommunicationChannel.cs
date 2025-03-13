using System.Net.Sockets;

namespace EnterTheNexus.Network.Abstractions;

public class SocketCommunicationChannel : ICommunicationChannel
{
    private readonly Socket _socket;
    public bool IsConnected => _socket.Connected;

    public SocketCommunicationChannel(Socket socket)
    {
        _socket = socket;
    }

    public async ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        return await _socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
    }

    public async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        await _socket.SendAsync(buffer, SocketFlags.None, cancellationToken);
    }

    public void Close()
    {
        if (!IsConnected) return;
        try
        {
            _socket.Close();
        }
        catch (Exception)
        {
            // Ignored;
        }

    }
}