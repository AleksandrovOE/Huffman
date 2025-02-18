using System;

namespace Huffman;

public class Huffman
{
    const int ByteCountersCount = byte.MaxValue + 1;
    public readonly byte[] Buffer;
    int BufferByteCount;
    readonly BitBuffer BitBuffer = new();
    readonly uint[] Counters;
    byte[]? Data = null;
    readonly HuffmanTree Tree;
    public int CountByteInBuffer { get; private set; }
    public long CountBitsInBuffer { get; private set; }

    public bool Encode(byte[] data, int size)
    {
        CountByteInBuffer = 0; CountBitsInBuffer = 0;
        Data = data;
        CountData();
        Tree.Rebuild(Counters);
        Tree.CalculateCodes();
        if (Tree.GetCompressedSizeInBytes() > Buffer.Length) return false;
        BitBuffer.Reset(Buffer, size, Operation.Write);
        foreach (var b in Data) if(!BitBuffer.AddBits(Tree.Codes[b].BitLength, Tree.Codes[b].Bits)) return false;
        CountByteInBuffer = BitBuffer.TotalBytesCount; 
        CountBitsInBuffer = BitBuffer.TotalBitsCount;
        return true;
    }

    void CountData()
    {
        Array.Fill(Counters, 0u);
        if (Data is null) return;
        foreach (var b in Data) Counters[b]++;
    }

    public bool Decode(byte[] data, long totalBitsCount)
    {
        Tree.CalculateCodes();
        Tree.CalculateDecodeAccelerators();
        BitBuffer.Reset(data, (int)((totalBitsCount + 7) >> 3), Operation.Read);
        CountBitsInBuffer = CountByteInBuffer = 0;
        var node = HuffmanTree.TreeSize;
        var decodedBitsCount = (byte)0;
        var i = 0;
        while (totalBitsCount > 0 && i < Buffer.Length)
        {
            if (!BitBuffer.Get8Bits(decodedBitsCount, out var _8bit)) return false;
            node = Tree.DecodeBits(_8bit, out decodedBitsCount, node);
            totalBitsCount -= decodedBitsCount;
            if (node < HuffmanTree.HalfTreeSize) { Buffer[i++] = (byte)node; node = HuffmanTree.TreeSize; }
        }
        if (totalBitsCount > 0) return false;
        CountByteInBuffer = i; CountBitsInBuffer = i << 3;
        return true;
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
