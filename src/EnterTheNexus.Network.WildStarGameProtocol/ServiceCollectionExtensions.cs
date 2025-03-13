using EnterTheNexus.Network.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EnterTheNexus.Network.WildStarGameProtocol;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWildStarPacketSerializer(this IServiceCollection services) => services.AddScoped<IPacketSerializer<IWildStarPacket>, WildStarPacketSerializer>();
}