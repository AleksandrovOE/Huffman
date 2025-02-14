using System;

namespace Huffman;

public class Huffman
{
    const int ByteCount = byte.MaxValue + 1;
    readonly byte[] Buffer;
    readonly BitBuffer BitBuffer = new();
    int BufferByte;
    byte BufferBit;
    readonly ulong[] Counters;
    byte[]? Data = null;
    readonly HuffmanTree Tree;

    public bool Encode(byte[] data)
    {
        Data = data;
        CountData();
        Tree.Rebuild(Counters);
        Tree.CalculateCodes();
        BitBuffer.Reset(Buffer);
        if (Tree.GetCompressedSizeInBytes() > Buffer.Length) return false;
        foreach (var b in Data) BitBuffer.AddBits(Tree.Codes[b].BitLength, Tree.Codes[b].Bits);
        return true;
    }

    void CountData()
    {
        Array.Fill<ulong>(Counters, 0ul);
        if (Data is null) return;
        foreach (var b in Data) Counters[b]++;
    }

    public Huffman(int size)
    {
        Buffer = new byte[size];
        Counters = new ulong[ByteCount];
        Tree = new();
    }
    public Huffman():this(int.MaxValue >> 1) { }
}
