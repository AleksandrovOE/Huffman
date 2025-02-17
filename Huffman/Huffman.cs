using System;

namespace Huffman;

public class Huffman
{
    const int ByteCountersCount = byte.MaxValue + 1;
    readonly byte[] Buffer;
    readonly BitBuffer BitBuffer = new();
    int BufferByte;
    byte BufferBit;
    readonly uint[] Counters;
    byte[]? Data = null;
    readonly HuffmanTree Tree;
    public int CountByteInBuffer { get; private set; }
    public long CountBitsInBuffer { get; private set; }

    public bool Encode(byte[] data)
    {
        CountByteInBuffer = 0; CountBitsInBuffer = 0;
        Data = data;
        CountData();
        Tree.Rebuild(Counters);
        Tree.CalculateCodes();
        BitBuffer.Reset(Buffer);
        if (Tree.GetCompressedSizeInBytes() > Buffer.Length) return false;
        foreach (var b in Data) if(!BitBuffer.AddBits(Tree.Codes[b].BitLength, Tree.Codes[b].Bits)) return false;
        CountByteInBuffer = BitBuffer.TotalBytesCount; CountBitsInBuffer = BitBuffer.TotalBitsCount;
        return true;
    }

    void CountData()
    {
        Array.Fill<uint>(Counters, 0u);
        if (Data is null) return;
        foreach (var b in Data) Counters[b]++;
    }

    public Huffman(int size)
    {
        CountBitsInBuffer = CountByteInBuffer = 0;
        Buffer = new byte[size];
        Counters = new uint[ByteCountersCount];
        Tree = new();
    }
    public Huffman():this(int.MaxValue >> 1) { }
}
