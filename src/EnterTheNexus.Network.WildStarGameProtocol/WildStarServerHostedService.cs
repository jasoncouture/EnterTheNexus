using EnterTheNexus.Network.Abstractions;
using Microsoft.Extensions.Hosting;

namespace EnterTheNexus.Network.WildStarGameProtocol;

public class WildStarServerHostedService : BackgroundService
{
    private readonly IServer<IWildStarPacket> _server;

    public WildStarServerHostedService(IServer<IWildStarPacket> server)
    {
        _server = server;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
        => await _server.RunAsync(stoppingToken);
}