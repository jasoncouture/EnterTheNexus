using System.Buffers;
using System.Diagnostics;
using EnterTheNexus.Network.WildStarGameProtocol;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EnterTheNexus.Network.UnitTests;

public class PacketSerializerTests
{
    private static byte[] _packet;
    static PacketSerializerTests()
    {
        _packet = GeneratePacket();
    }
    private static byte[] GeneratePacket()
    {
        using var testStream = new MemoryStream();
        
        testStream.Seek(4, SeekOrigin.Begin);
        var bytes = BitConverter.GetBytes((ushort)PacketType.ClientCheat);
        testStream.Write(bytes, 0, bytes.Length);
        bytes = BitConverter.GetBytes(1);
        testStream.Write(bytes, 0, bytes.Length);
        bytes = BitConverter.GetBytes(2);
        testStream.Write(bytes, 0, bytes.Length);
        bytes = BitConverter.GetBytes(3);
        testStream.Write(bytes, 0, bytes.Length);
        bytes = BitConverter.GetBytes((int)testStream.Position);
        testStream.WriteByte(0);
        testStream.Seek(0, SeekOrigin.Begin);
        testStream.Write(bytes, 0, bytes.Length);
        testStream.Flush();
        
        return testStream.ToArray();
    }
    [Fact]
    public void PacketShouldDeserializeCorrectly()
    {
        var sequence = new ReadOnlySequence<byte>(_packet.AsMemory());
        var serviceProvider = new ServiceCollection()
            .AddWildStarPacketSerializer()
            .BuildServiceProvider();
        
        var serializer = new WildStarPacketSerializer(serviceProvider);
        serializer.TryDeserialize(sequence, out var packet, out var length).ShouldBeTrue();
        length.ShouldBe(_packet.Length - 1);
        var inboundPacket = packet.ShouldBeOfType<TestPacket>();
        inboundPacket.Field3.ShouldBe(1);
        inboundPacket.Field2.ShouldBe(2);
        inboundPacket.Field1.ShouldBe(3);
        inboundPacket.IgnoredField.ShouldBe("Ignored");
    }
}