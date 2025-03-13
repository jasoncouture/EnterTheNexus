using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace EnterTheNexus.Network.WildStarGameProtocol;

public record PropertyData(int? Bits, PropertyInfo PropertyInfo, int Order)
{
    public string Name => PropertyInfo.Name;

    private static readonly ConcurrentDictionary<Type, Func<BitReader, PropertyData, object>>
        ConverterCache = new ConcurrentDictionary<Type, Func<BitReader, PropertyData, object>>();

    private static readonly MethodInfo GenericConverterMethod;

    static PropertyData()
    {
        var method = typeof(PropertyData).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(i => i.Name == nameof(ConvertToType));
        GenericConverterMethod = method;
    }
    private static Func<BitReader, PropertyData, object> GetConverterInternal(Type type)
    {
        return (Func<BitReader, PropertyData, object>)Delegate.CreateDelegate(typeof(Func<BitReader, PropertyData, object>),
            GenericConverterMethod.MakeGenericMethod(type));
    }

    private static Func<BitReader, PropertyData, object> GetConverter(Type type)
    {
        return ConverterCache.GetOrAdd(type, GetConverterInternal);
    }

    private static object ConvertToType<T>(BitReader reader, PropertyData propertyData)
        where T : struct, INumber<T>
    {
        var length = propertyData.Bits ?? (Marshal.SizeOf<T>() * 8);
        if (typeof(T) == typeof(byte))
        {
            return reader.Read<byte>(length, i => i[0]);
        }

        if (typeof(T) == typeof(short))
        {
            return reader.Read(length, BitConverter.ToInt16);
        }

        if (typeof(T) == typeof(ushort))
        {
            return reader.Read(length, BitConverter.ToUInt16);
        }

        if (typeof(T) == typeof(int))
        {
            return reader.Read(length, BitConverter.ToInt32);
        }

        if (typeof(T) == typeof(uint))
        {
            return reader.Read(length, BitConverter.ToUInt32);
        }

        if (typeof(T) == typeof(long))
        {
            return reader.Read(length, BitConverter.ToInt64);
        }

        if (typeof(T) == typeof(ulong))
        {
            return reader.Read(length, BitConverter.ToInt64);
        }

        if (typeof(T) == typeof(Half))
        {
            return reader.Read(length, BitConverter.ToUInt64);
        }

        if (typeof(T) == typeof(float))
        {
            return reader.Read(length, BitConverter.ToSingle);
        }

        if (typeof(T) == typeof(double))
        {
            return reader.Read(length, BitConverter.ToDouble);
        }

        ThrowUnsupportedProperty(propertyData.PropertyInfo);
        return default!; // Unreachable.
    }

    private static void ThrowUnsupportedProperty(PropertyInfo propertyInfo)
    {
        throw new NotSupportedException(
            $"Unsupported property type for property '{propertyInfo.DeclaringType!.Name}.{propertyInfo.Name}': {propertyInfo.PropertyType}");
    }

    public void SetFromReader(BitReader reader, IWildStarPacket packet, WildStarPacketSerializer serializer,
        PacketType packetType)
    {
        if (PropertyInfo.PropertyType.IsAssignableTo(typeof(IWildStarPacket)))
        {
            var innerField = serializer.Deserialize(reader, PropertyInfo.PropertyType, packetType);
            Debug.Assert(innerField.GetType().IsAssignableTo(PropertyInfo.PropertyType));
            PropertyInfo.SetValue(packet, innerField);
        }
        else if (PropertyInfo.PropertyType.IsAssignableTo(typeof(PacketType)))
        {
            PropertyInfo.SetValue(packet, packetType);
        }
        else if (PropertyInfo.PropertyType is { IsValueType: true, IsPrimitive: true })
        {
            var converter = GetConverter(PropertyInfo.PropertyType);
            PropertyInfo.SetValue(packet, converter.Invoke(reader, this));
        }
        else if (PropertyInfo.PropertyType == typeof(byte[]))
        {
            Span<byte> data = stackalloc byte[(Bits ?? 8 + 1) / 8];
            reader.ReadBitSequence(data, Bits ?? 8, rawOrder: true);
            PropertyInfo.SetValue(packet, data.ToArray());
        }
        else if (PropertyInfo.PropertyType == typeof(string))
        {
            var value = Bits switch
            {
                16 => reader.ReadString(true, Encoding.Unicode),
                null => reader.ReadString(false, Encoding.Unicode),
                _ => reader.ReadString(reader.Read(Bits.Value, BitConverter.ToInt32), Encoding.Unicode)
            };
            PropertyInfo.SetValue(packet, value);
        }
        else
        {
            ThrowUnsupportedProperty(PropertyInfo);
        }
    }
}