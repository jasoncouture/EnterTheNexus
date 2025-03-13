using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EnterTheNexus.Network.Abstractions;

public static class PacketContextFactory
{
    private static readonly MethodInfo CreateInternalGenericMethodInfo = typeof(PacketContextFactory)
        .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
        .Single(i => i.Name == nameof(CreateInternal));

    private static readonly ConcurrentDictionary<Type, MethodInfo> MethodCache =
        new ConcurrentDictionary<Type, MethodInfo>();

    [DynamicDependency(
        DynamicallyAccessedMemberTypes.NonPublicMethods |
        DynamicallyAccessedMemberTypes.NonPublicFields,
        typeof(PacketContextFactory)
    )]
    private static readonly ThreadLocal<object[]> ParameterCache = new ThreadLocal<object[]>(() => new object[2]);

    public static IPacketContext<T> Create<T>(object packet, IServerClient<T> serverClient) where T : class
    {
        var packetType = packet.GetType();
        var method = MethodCache.GetOrAdd(
            packetType,
            static t => CreateInternalGenericMethodInfo.MakeGenericMethod(t)
        );
        var args = ParameterCache.Value ??= new object[2];
        args[0] = packet;
        args[1] = serverClient;
        return (IPacketContext<T>)method.Invoke(null, args)!;
    }

    private static PacketContext<T> CreateInternal<T>(T packet, IServerClient<T> serverClient) where T : class
    {
        return new PacketContext<T>(packet, serverClient);
    }
}