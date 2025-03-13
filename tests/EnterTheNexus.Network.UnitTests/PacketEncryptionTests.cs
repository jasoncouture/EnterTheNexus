using EnterTheNexus.Network.Abstractions;
using Shouldly;

namespace EnterTheNexus.Network.UnitTests;

public class PacketEncryptionTests
{
    [Fact]
    public void WhenNoKeyIsSetNoEncryptionIsUsed()
    {
        var buffer = new Memory<byte>(new byte[1024]);
        var packetEncryption = PacketEncryption.Create();
        packetEncryption.Encrypt(buffer);
        buffer.Span.ToArray().ShouldNotContain(i => i != 0);
        packetEncryption.Decrypt(buffer);
        buffer.Span.ToArray().ShouldNotContain(i => i != 0);
    }

    [Fact]
    public void CanDecryptOwnOutputWhenKeyIsSet()
    {
        var expected = new byte[1024];
        var buffer = new Memory<byte>(new byte[1024]);
        var packetEncryption = PacketEncryption.CreateDefault();
        packetEncryption.Encrypt(buffer);
        buffer.Span.ToArray().ShouldSatisfyAllConditions(i => i.All(x => x == 0).ShouldBeFalse());
        packetEncryption.Decrypt(buffer);
        buffer.Span.ToArray().ShouldBe(expected);
    }
}