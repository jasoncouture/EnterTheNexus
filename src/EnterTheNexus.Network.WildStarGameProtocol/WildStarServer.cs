using EnterTheNexus.Network.Abstractions;
using Microsoft.Extensions.Options;

namespace EnterTheNexus.Network.WildStarGameProtocol;

public class WildStarServer : Server<IWildStarPacket>
{
    public WildStarServer(IOptionsMonitor<WildStarServerOptions> optionsMonitor, IServerClientFactory<IWildStarPacket> clientFactory, IServiceProvider serviceProvider) : base(optionsMonitor, clientFactory, serviceProvider)
    {
    }
}