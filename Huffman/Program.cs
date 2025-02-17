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


        var hf = new Huffman(10000);
        var b = new byte[9000];
        var n = b.Length; while (n-- > 0) b[n] = (byte)Random.Shared.Next(256);
        hf.Encode(b);
        Console.WriteLine($"размер = {hf.CountByteInBuffer} байт, {hf.CountBitsInBuffer} бит");
    }
}
