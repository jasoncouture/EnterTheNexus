using System.IO.Pipelines;

namespace EnterTheNexus.Network.Abstractions;

public interface IPipeFactory<TPacket>
{
    Pipe CreatePipe();
}