using System.Buffers;
using BenchmarkDotNet.Attributes;
using EnterTheNexus.Network.Abstractions;
using EnterTheNexus.Network.WildStarGameProtocol;
using Microsoft.Extensions.DependencyInjection;

[MemoryDiagnoser]
public class Benchmarks
{
    private static readonly ReadOnlyMemory<byte> Packet;

    private static readonly IServiceProvider ServiceProvider = new ServiceCollection()
        .AddWildStarPacketSerializer()
        .BuildServiceProvider();

    static Benchmarks()
    {
        Packet = GeneratePacket();
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

    [Benchmark]
    public void PacketParserTests()
    {
        using var scope = ServiceProvider.CreateScope();
        var sequence = new ReadOnlySequence<byte>(Packet);
        scope.ServiceProvider
            .GetRequiredService<IPacketSerializer<IWildStarPacket>>()
            .TryDeserialize(sequence, out _, out _);
    }
}