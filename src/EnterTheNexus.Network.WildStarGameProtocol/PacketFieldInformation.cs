using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Serialization;

namespace EnterTheNexus.Network.WildStarGameProtocol;

public static class PacketFieldInformation
{
    private static readonly ConcurrentDictionary<Type, ImmutableList<PropertyData>> PropertyDataCache =
        new ConcurrentDictionary<Type, ImmutableList<PropertyData>>();

    public static IEnumerable<PropertyData> GetPropertyData(Type type)
    {
        return PropertyDataCache.GetOrAdd(type, GetPropertyDataInternal);
    }

    private static ImmutableList<PropertyData> GetPropertyDataInternal(Type type)
    {
        return EnumerateProperties(type).ToImmutableList();
    }

    private static IEnumerable<PropertyData> EnumerateProperties(Type type)
    {
        var count = 0;
        foreach (var propertyInfo in type.GetProperties()
                     .Where(i => i is { CanRead: true, CanWrite: true }
                     )
                )
        {
            if (propertyInfo.GetCustomAttribute<IgnoreDataMemberAttribute>() is not null)
            {
                continue;
            }

            var order = (count * 10) + 100;

            var dataMemberAttribute = propertyInfo.GetCustomAttribute<DataMemberAttribute>();
            if (dataMemberAttribute is not null)
            {
                if (dataMemberAttribute.Order != 0)
                {
                    order = dataMemberAttribute.Order;
                    count--;
                }
            }

            if (propertyInfo.PropertyType.IsAssignableTo(typeof(IWildStarPacket)))
            {
                yield return new PropertyData(null, propertyInfo, order);
                count++;
                continue;
            }

            var attribute = propertyInfo.GetCustomAttribute<BitSizeAttribute>();
            yield return new PropertyData(attribute?.Bits, propertyInfo, order);
            count++;
        }
    }
}