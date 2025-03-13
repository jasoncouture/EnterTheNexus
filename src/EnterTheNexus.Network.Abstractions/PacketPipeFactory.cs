using System.IO.Pipelines;

namespace EnterTheNexus.Network.Abstractions;

public class PacketPipeFactory<TPacket> : IPipeFactory<TPacket> where TPacket : class
{
    private static PipeOptions CreatePipeOptions()
    {
        return new PipeOptions(
            pauseWriterThreshold: 1024 * 1024 * 2,
            resumeWriterThreshold: 1024 * 1024 * 2,
            useSynchronizationContext: false
        );
    }

    public Pipe CreatePipe()
    {
        return new Pipe(CreatePipeOptions());
    }
}