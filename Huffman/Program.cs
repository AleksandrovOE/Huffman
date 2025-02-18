namespace Huffman;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine($"Hello, World! {1<<2}");
        var ht = new HuffmanTree();
        var a = new uint[256];
        var i = 256; while (i-- > 0) a[i] = (uint)Random.Shared.Next(100000);
        ht.Rebuild(a);
        ht.CheckTree(true);
        ht.CalculateCodes();
        ht.CheckCodes(true);
        ht.CalculateDecodeAccelerators();
        ht.CheckDecodeAccelerators(true);
        var x = ht.DecodeBits(121, out var decodedBitsCount);
        Console.WriteLine($"x = {x}, decodedBitsCount = {decodedBitsCount}");


        Huffman hf = new(10000);
        var dataBuffer = new byte[9000];
        var n = dataBuffer.Length; while (n-- > 0) dataBuffer[n] = (byte)Random.Shared.Next(256);
        var res = hf.Encode(dataBuffer, dataBuffer.Length);
        Console.WriteLine($"{res} размер = {hf.CountByteInBuffer} байт, {hf.CountBitsInBuffer} бит");

        var bitsCount = hf.CountBitsInBuffer;
        var EncodedBuffer = (byte[])hf.Buffer.Clone();
        var resD = hf.Decode(EncodedBuffer, bitsCount);
    }
}
