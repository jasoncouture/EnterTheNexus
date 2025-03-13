using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnterTheNexus.Network.Abstractions;

public static class PacketProcessorServiceCollectionExtensions
{
    public static IServiceCollection AddPacketSerializer<TPacket, TSerializer>(this IServiceCollection services)
        where TSerializer : class, IPacketSerializer<TPacket>
        where TPacket : class
    {
        services.AddScoped<IPacketSerializer<TPacket>, TSerializer>();
        return services;
    }

    public static IServiceCollection AddPacketProcessingServices(this IServiceCollection services)
    {
        services.TryAddScoped(typeof(IPacketContextAccessor<>), typeof(PacketContextAccessor<>));
        services.TryAddScoped(typeof(IServerClient<>), typeof(PipelineServerClient<>));
        return services;
    }

    public static IServiceCollection AddPacketHandler<TPacket, TService>(this IServiceCollection services)
        where TService : class,
        IPacketHandler<TPacket> where TPacket : class
    {
        services.TryAddScoped<IPacketHandler<TPacket>, TService>();
        return services;
    }
}