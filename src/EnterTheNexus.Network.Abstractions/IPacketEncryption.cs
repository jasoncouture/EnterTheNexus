using System.Runtime.CompilerServices;

namespace EnterTheNexus.Network.Abstractions;

public interface IPacketEncryption
{
    void SetEncryptionKey(ulong keyInteger);
    void SetEncryptionKey(ReadOnlySpan<byte> ticket);
    void Encrypt(Memory<byte> buffer, int offset, int length);
    void Encrypt(Memory<byte> buffer, int offset) => Encrypt(buffer, offset, buffer.Length - offset);
    void Encrypt(Memory<byte> buffer) => Encrypt(buffer, 0, buffer.Length);
    void Decrypt(Memory<byte> buffer, int offset, int length);
    void Decrypt(Memory<byte> buffer, int offset) => Decrypt(buffer, offset, buffer.Length - offset);
    void Decrypt(Memory<byte> buffer) => Decrypt(buffer, 0, buffer.Length);
}