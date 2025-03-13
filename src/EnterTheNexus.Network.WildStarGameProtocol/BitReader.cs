using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace EnterTheNexus.Network.WildStarGameProtocol;

public class BitReader
{
    public BitPosition Position { get; }
    public ReadOnlySequence<byte> Buffer { get; private set; }

    public BitReader Reset()
    {
        return SetBuffer(ReadOnlySequence<byte>.Empty);
    }

    public BitReader SetBuffer(ReadOnlySequence<byte> buffer)
    {
        Position.Reset();
        Buffer = buffer;
        return this;
    }

    public BitReader(BitPosition position)
    {
        Position = position;
    }

    public bool ReadBit()
    {
        var slice = Position.GetSlice(Buffer);
        var ret = ReadBitInternal(slice, Position);
        Position.AdvanceBits(1);
        return ret;
    }

    private static bool ReadBitInternal(ReadOnlySequence<byte> buffer, BitPosition position)
    {
        if (buffer.Length == 0)
        {
            return false;
        }

        return ReadBitInternal(ReadByteInternal(buffer), position);
    }

    public byte ReadBits(int bits)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bits, 8);
        ArgumentOutOfRangeException.ThrowIfLessThan(bits, 1);
        byte value = 0;
        for (var i = 0; i < bits; i++)
        {
            if (ReadBit())
                value |= (byte)(1ul << i);
        }

        return value;
    }

    public void ReadBitSequence(Span<byte> bytes, int bits, bool rawOrder = false)
    {
        var counter = 0;
        ArgumentOutOfRangeException.ThrowIfLessThan(bits, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bits, bytes.Length * 8);
        while (bits > 0)
        {
            var next = bits > 8 ? 8 : bits;
            bits -= next;
            int index = 0;
            if (!rawOrder && !BitConverter.IsLittleEndian)
            {
                index = bytes.Length - 1 - counter;
            }
            else
            {
                index = counter;
            }
            // Read in reverse, for LittleEndian conversion.

            bytes[index] = ReadBits(next);
            counter++;
        }
    }

    public string ReadString(int length, Encoding? encoding = null)
    {
        // Most, if not all strings are UTF16 in WildStar, so we'll default to that.
        encoding ??= Encoding.Unicode;
        Span<byte> buffer = stackalloc byte[length];
        ReadBitSequence(buffer, length * 8, rawOrder: true);
        return encoding.GetString(buffer);
    }

    public string ReadString(bool hasFixedLengthPrefix = false, Encoding? encoding = null)
    {
        var extended = ReadBit();
        var length = 0;
        if (hasFixedLengthPrefix)
        {
            length = Read(16, BitConverter.ToInt32) * 2;
        }
        else
        {
            length = Read((extended ? 15 : 7) << 1, BitConverter.ToInt32);
        }

        return ReadString(length, encoding);
    }

    public T Read<T>(Func<ReadOnlySpan<byte>, T> converter) where T : struct, INumber<T>
    {
        var size = Marshal.SizeOf<T>();
        return Read(size * 8, converter);
    }

    public T Read<T>(int bits, Func<ReadOnlySpan<byte>, T> converter) where T : struct, INumber<T>
    {
        var size = Marshal.SizeOf<T>();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bits, size * 8);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(bits, 0);

        Span<byte> data = stackalloc byte[size];
        ReadBitSequence(data, bits);
        return converter(data);
    }


    private static bool ReadBitInternal(byte data, BitPosition position)
    {
        return ((data >> position.BitOffset) & 1) != 0;
    }

    private static byte ReadByteInternal(ReadOnlySequence<byte> buffer)
    {
        return buffer.FirstSpan[0];
    }
}

// Reference from NexusForever - https://github.com/NexusForever/NexusForever

// public class GamePacketReader : IDisposable
// {
//     public uint BytePosition
//     {
//         get => (uint)(stream?.Position ?? 0u);
//         set
//         {
//             stream.Position = value;
//             ResetBits();
//         }
//     }
//
//     public uint BytesRemaining => stream?.Remaining() ?? 0u;
//
//     private byte currentBitPosition;
//     private byte currentBitValue;
//     private readonly Stream stream;
//
//     public GamePacketReader(Stream input)
//     {
//         stream = input;
//         ResetBits();
//     }
//
//     public void Dispose()
//     {
//         stream?.Dispose();
//     }
//
//     public void ResetBits()
//     {
//         if (currentBitPosition > 7)
//             return;
//
//         currentBitPosition = 8;
//         currentBitValue = 0;
//     }
//
//     public bool ReadBit()
//     {
//         currentBitPosition++;
//         if (currentBitPosition > 7)
//         {
//             currentBitPosition = 0;
//             currentBitValue = (byte)stream.ReadByte();
//         }
//
//         return ((currentBitValue >> currentBitPosition) & 1) != 0;
//     }
//
//     private ulong ReadBits(uint bits)
//     {
//         ulong value = 0ul;
//         for (uint i = 0u; i < bits; i++)
//             if (ReadBit())
//                 value |= 1ul << (int)i;
//
//         return value;
//     }
//
//     public byte ReadByte(uint bits = 8u)
//     {
//         if (bits > sizeof(byte) * 8)
//             throw new ArgumentException();
//
//         return (byte)ReadBits(bits);
//     }
//
//     public ushort ReadUShort(uint bits = 16u)
//     {
//         if (bits > sizeof(ushort) * 8)
//             throw new ArgumentException();
//
//         return (ushort)ReadBits(bits);
//     }
//
//     public short ReadShort(uint bits = 16u)
//     {
//         if (bits > sizeof(short) * 8)
//             throw new ArgumentException();
//
//         return (short)ReadBits(bits);
//     }
//
//     public uint ReadUInt(uint bits = 32u)
//     {
//         if (bits > sizeof(uint) * 8)
//             throw new ArgumentException();
//
//         return (uint)ReadBits(bits);
//     }
//
//     public int ReadInt(uint bits = 32u)
//     {
//         if (bits > sizeof(int) * 8)
//             throw new ArgumentException();
//
//         return (int)ReadBits(bits);
//     }
//
//     public float ReadSingle(uint bits = 32u)
//     {
//         if (bits > sizeof(float) * 8)
//             throw new ArgumentException();
//
//         int value = (int)ReadBits(bits);
//         return BitConverter.Int32BitsToSingle(value);
//     }
//
//     public double ReadDouble(uint bits = 64u)
//     {
//         if (bits > sizeof(double) * 8)
//             throw new ArgumentException();
//
//         long value = (long)ReadBits(bits);
//         return BitConverter.Int64BitsToDouble(value);
//     }
//
//     public ulong ReadULong(uint bits = 64u)
//     {
//         if (bits > sizeof(ulong) * 8)
//             throw new ArgumentException();
//
//         return ReadBits(bits);
//     }
//
//     public T ReadEnum<T>(uint bits = 64u) where T : Enum
//     {
//         if (bits > sizeof(ulong) * 8)
//             throw new ArgumentException();
//
//         return (T)Enum.ToObject(typeof(T), ReadBits(bits));
//     }
//
//     public byte[] ReadBytes(uint length)
//     {
//         byte[] data = new byte[length];
//         for (uint i = 0u; i < length; i++)
//             data[i] = ReadByte();
//
//         return data;
//     }
//
//     // public string ReadWideStringFixed()
//     // {
//     //     ushort length = ReadUShort();
//     //     byte[] data = ReadBytes(length * 2u);
//     //     return Encoding.Unicode.GetString(data, 0, data.Length - 2);
//     // }
//     //
//     // public string ReadWideString()
//     // {
//     //     bool extended = ReadBit();
//     //     ushort length = (ushort)(ReadUShort(extended ? 15u : 7u) << 1);
//     //
//     //     byte[] data = ReadBytes(length);
//     //     return Encoding.Unicode.GetString(data);
//     // }
//
//     public float ReadPackedFloat()
//     {
//         float UnpackFloat(ushort packed)
//         {
//             uint v3 = packed & 0xFFFF7FFF;
//             uint v4 = (packed & 0xFFFF8000) << 16;
//
//             if ((v3 & 0x7C00) != 0)
//                 return BitConverter.Int32BitsToSingle((int)(v4 | ((v3 + 0x1C000) << 13)));
//             if ((v3 & 0x3FF) == 0)
//                 return BitConverter.Int32BitsToSingle((int)(v4 | v3));
//
//             uint v6 = (v3 & 0x3FF) << 13;
//             uint i = 113;
//             for (; v6 <= 0x7FFFFF; --i)
//                 v6 *= 2;
//             return BitConverter.Int32BitsToSingle((int)(v4 | (i << 23) | v6 & 0x7FFFFF));
//         }
//
//         return UnpackFloat(ReadUShort());
//     }
//
//     // public Vector3 ReadVector3()
//     // {
//     //     return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
//     // }
//     //
//     // public Vector3 ReadPackedVector3()
//     // {
//     //     return new Vector3(ReadPackedFloat(), ReadPackedFloat(), ReadPackedFloat());
//     // }
// }