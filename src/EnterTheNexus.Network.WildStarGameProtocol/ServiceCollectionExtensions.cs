using EnterTheNexus.Network.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnterTheNexus.Network.WildStarGameProtocol;

public static class ServiceCollectionExtensions
{
    private const string DefaultConfigurationSectionName = "WildStar:Server";
    public static IServiceCollection AddWildStarServer(this IServiceCollection services, string serverConfigurationSectionName = DefaultConfigurationSectionName)
    {
        services.AddPacketProcessingServices();
        services.AddWildStarPacketSerializer();
        services.AddOptions<WildStarServerOptions>()
            .BindConfiguration(serverConfigurationSectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IServer<IWildStarPacket>, WildStarServer>();
        services.AddHostedService<WildStarServerHostedService>();
        return services;
    }

    public static IServiceCollection AddWildStarPacketSerializer(this IServiceCollection services)
    {
        services.TryAddScoped<IPacketSerializer<IWildStarPacket>, WildStarPacketSerializer>();
        return services;
    }
}