using EnterTheNexus.Network.Abstractions;

namespace EnterTheNexus.Network.WildStarGameProtocol;

public class WildStarServerOptions : ServerOptions {
    public WildStarServerOptions()
    {
        ListenUri = new Uri("tcp://localhost:24000");
    }
}