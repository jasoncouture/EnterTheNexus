namespace EnterTheNexus.Network.Abstractions;

public class PacketEncryption : IPacketEncryption
{
    private ulong? _keyValue;
    private readonly byte[] _key = new byte[CryptKeySize];

    public void ClearEncryptionKey()
    {
        _keyValue = null;
        Array.Clear(_key, 0, _key.Length);
    }

    public void SetEncryptionKey(ulong keyInteger)
    {
        var keyVal = CryptKeyInitialValue;
        var v2 = (keyVal + keyInteger) * CryptMultiplier;
        Span<byte> v2Buffer = stackalloc byte[sizeof(ulong)];
        var keyBuffer = new Span<byte>(_key);
        for (var i = 0; i < CryptKeySize; i += 8)
        {
            BitConverter.TryWriteBytes(v2Buffer, v2);
            v2Buffer.CopyTo(keyBuffer[i..]);
            keyVal = (keyVal + v2) * CryptMultiplier;
            v2 = (keyInteger + v2) * CryptMultiplier;
        }

        _keyValue = keyVal;
    }

    public void SetEncryptionKey(ReadOnlySpan<byte> ticket)
    {
        var keyInteger = GetKeyFromTicket(ticket);
        SetEncryptionKey(keyInteger);
    }


    public static ulong GetKeyFromTicket(ReadOnlySpan<byte> sessionKey)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(sessionKey.Length, 16, nameof(sessionKey));

        var key = CryptKeyInitialValue;
        for (var i = 0; i < 16; i++)
            key = (key + sessionKey[i]) * CryptMultiplier;

        return (key + CryptBaseKey) * CryptMultiplier;
    }

    public void Encrypt(Memory<byte> buffer, int offset, int length)
    {
        HandleEncryptionInternal(buffer.Slice(offset, length), true);
    }

    public void Decrypt(Memory<byte> buffer, int offset, int length)
    {
        HandleEncryptionInternal(buffer.Slice(offset, length), false);
    }

    private void HandleEncryptionInternal(Memory<byte> memory, bool encrypt)
    {
        if (_keyValue is null)
        {
            return;
        }

        Span<byte> state = stackalloc byte[sizeof(ulong)];
        BitConverter.TryWriteBytes(state, _keyValue.Value);
        if (!encrypt)
        {
            state.Reverse();
        }

        var encryptionState = CryptMultiplier2 * (uint)memory.Length;
        var keyBaseOffset = 0u;

        for (var i = 0; i < memory.Length; i++)
        {
            var keyIndex = i % 8;
            if (keyIndex == 0)
            {
                keyBaseOffset = (encryptionState & 0xF) * 8;
                encryptionState += 1;
            }

            var beforePass = memory.Span[i];
            var stateIndex = encrypt ? keyIndex : (7 - keyIndex);
            memory.Span[i] = (byte)(state[stateIndex] ^ memory.Span[i] ^ _key[keyBaseOffset + keyIndex]);
            if (!encrypt)
            {
                state[stateIndex] = beforePass;
            }
            else
            {
                state[stateIndex] = memory.Span[i];
            }
        }
    }


    private const int CryptKeyBitSize = 1024;
    private const int CryptKeySize = CryptKeyBitSize / 8;
    private const uint CryptMultiplier2 = 0xAA7F8EAAu;
    private const ulong CryptMultiplier = 0xAA7F8EA9u;
    private const ulong CryptKeyInitialValue = 0x718DA9074F2DEB91u;
    private const ulong Build = 16042;
    private static readonly ulong CryptBaseKey = ComputeBaseKey();

    static ulong ComputeBaseKey()
    {
        unchecked
        {
            var key = CryptKeyInitialValue + 0x5B88D61139619662;
            key = key * CryptMultiplier;
            key = (key + Build) * CryptMultiplier;
            return (key + 0x97998A0) * CryptMultiplier;
        }
    }

    public static IPacketEncryption Create(ulong keyInteger)
    {
        var ret = new PacketEncryption();
        ret.SetEncryptionKey(keyInteger);
        return ret;
    }

    public static IPacketEncryption Create(ReadOnlySpan<byte> ticket)
    {
        var ret = new PacketEncryption();
        ret.SetEncryptionKey(ticket);
        return ret;
    }

    public static IPacketEncryption Create()
    {
        return new PacketEncryption();
    }

    public static IPacketEncryption CreateDefault()
    {
        return Create(CryptBaseKey);
    }


    // ported from NexusForever, original encryption code:
    // public byte[] Decrypt(byte[] buffer, int length)
    // {
    //     byte[] outputBytes = new byte[length];
    //     byte[] state = BitConverter.GetBytes(keyValue).Reverse().ToArray();
    //
    //     uint v4 = CryptMultiplier2 * (uint)length;
    //     uint v9 = 0;
    //
    //     for (int i = 0; i < length; i++)
    //     {
    //         int stateIndex = i % 8;
    //         if (stateIndex == 0) // each 8 iteration.
    //             v9 = (v4++ & 0xF) * 8;
    //
    //         byte test = (byte) (state[7 - stateIndex] ^ buffer[i] ^ key[v9 + stateIndex]);
    //         outputBytes[i] = test;
    //
    //         // only difference between encrypt and decrypt
    //         state[7 - stateIndex] = buffer[i];
    //     }
    //
    //     return outputBytes;
    // }
    //
    // public byte[] Encrypt(byte[] buffer, int length)
    // {
    //     byte[] outputBytes = new byte[length];
    //     Span<byte> state = stackalloc byte[sizeof(ulong)];
    //     BitConverter.TryWriteBytes(state, keyValue);
    //
    //     uint v4 = CryptMultiplier2 * (uint)length;
    //     uint v9 = 0;
    //
    //     for (int i = 0; i < length; i++)
    //     {
    //         int stateIndex = i % 8;
    //         if (stateIndex == 0) // each 8 iteration.
    //             v9 = (v4++ & 0xF) * 8;
    //
    //         outputBytes[i] = (byte)(state[stateIndex] ^ buffer[i] ^ key[v9 + stateIndex]);
    //
    //         // only difference between encrypt and decrypt.
    //         state[stateIndex] = outputBytes[i];
    //     }
    //
    //     return outputBytes;
    // }
}