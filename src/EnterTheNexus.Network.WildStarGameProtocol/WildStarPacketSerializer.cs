using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using EnterTheNexus.Network.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EnterTheNexus.Network.WildStarGameProtocol;

public class WildStarPacketSerializer : IPacketSerializer<IWildStarPacket>
{
    private readonly IServiceProvider _serviceProvider;

    private static readonly ThreadLocal<BitReader> BitPositionCache =
        new ThreadLocal<BitReader>(() => new BitReader(new BitPosition()));

    private BitReader BitReader => BitPositionCache.Value ??= new BitReader(new BitPosition());
    private static readonly ConcurrentDictionary<PacketType, Type> PacketTypes;

    private static readonly ConcurrentDictionary<Type, PacketType?> PacketTypeCache =
        new ConcurrentDictionary<Type, PacketType?>();

    static WildStarPacketSerializer()
    {
        PacketTypes = new ConcurrentDictionary<PacketType, Type>();
        var assemblies = AssemblyLoadContext.Default.Assemblies
            .Where(IsNotSystemAssembly)
            .OrderBy(i => i.GetName().Name == "EnterTheNexus.Network.WildStarGameProtocol" ? 0 : 1)
            .ThenBy(i => i.GetName().Name!.Length)
            .ThenBy(i => i.GetName().Name!)
            .ToImmutableList();
        foreach (var assembly in assemblies)
        {
            RegisterTypesFromAssembly(assembly);
        }
    }

    private static bool IsNotSystemAssembly(Assembly assembly)
    {
        var name = assembly.GetName().Name;
        if (name is null)
        {
            return false;
        }

        if (assembly.IsDynamic)
        {
            return false;
        }

        if (name.StartsWith("System.") || name.StartsWith("Microsoft."))
        {
            return false;
        }

        return true;
    }

    private static void RegisterTypesFromAssembly(Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(i => i.IsAssignableTo(typeof(IWildStarPacket)))
            .Where(i => i.IsClass && !i.IsAbstract);

        foreach (var type in types)
        {
            RegisterTypeInternal(type);
        }
    }

    private static void RegisterTypeInternal(Type type)
    {
        var packetTypeAttribute = type.GetCustomAttribute<PacketTypeAttribute>();
        PacketTypeCache[type] = packetTypeAttribute?.Type;
        if (packetTypeAttribute is not null)
        {
            PacketTypes[packetTypeAttribute.Type] = type;
        }
    }

    public WildStarPacketSerializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static PacketType? GetPacketType(Type type)
    {
        return PacketTypeCache.GetOrAdd(type, static t => t.GetCustomAttribute<PacketTypeAttribute>()?.Type);
    }

    private static Type? GetPacketType(PacketType packetType)
    {
        return PacketTypes.GetValueOrDefault(packetType);
    }

    public int GetEstimatedPacketLength(IWildStarPacket packet)
    {
        throw new NotImplementedException();
    }

    public int Serialize(IWildStarPacket packet, Memory<byte> memory)
    {
        throw new NotImplementedException();
    }

    internal IWildStarPacket Deserialize(BitReader bitReader, Type type, PacketType packetType)
    {
        var packetImplementation = CreatePacket(type);
        if (packetImplementation.CanDeserializeSelf)
        {
            packetImplementation.Deserialize(bitReader);
            return packetImplementation;
        }

        var fields = PacketFieldInformation.GetPropertyData(type);
        foreach (var field in fields.OrderBy(i => i.Order))
        {
            field.SetFromReader(bitReader, packetImplementation, this, packetType);
        }

        return packetImplementation;
    }

    public bool TryDeserialize(ReadOnlySequence<byte> buffer, [NotNullWhen(true)] out IWildStarPacket? packet,
        out int length)
    {
        packet = null;
        length = 0;
        if (buffer.Length < 6)
        {
            return false;
        }

        var bitReader = BitReader.SetBuffer(buffer);
        length = bitReader.Read(BitConverter.ToInt32);

        if (length < 6)
        {
            throw new InvalidOperationException(
                "Malformed packet: Length is less than 6 (4 length bytes + 2 type bytes)?");
        }

        if (buffer.Length < length)
        {
            return false;
        }

        if (length == 0)
        {
            throw new InvalidOperationException("Packet length is 0?");
        }

        var fieldTypeRaw = bitReader.Read(BitConverter.ToUInt16);
        var packetType = (PacketType)fieldTypeRaw;
        var implementationType = GetPacketType(packetType);
        if (implementationType is null)
        {
            throw new InvalidOperationException($"Unable to resolve CLR type for packet type: {packetType}");
        }

        packet = Deserialize(bitReader, implementationType, packetType);
        return true;
    }

    private IWildStarPacket CreatePacket(Type type)
    {
        return (IWildStarPacket)ActivatorUtilities.CreateInstance(_serviceProvider, type);
    }
}