namespace Huffman;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine($"Hello, World! {1<<2}");
        var ht = new HuffmanTree();
        var a = new ulong[256];
        var i = 256; while (i-- > 0) a[i] = (uint)Random.Shared.Next(256);
        ht.Rebuild(a);
        ht.CheckTree(true);
        ht.CalculateCodes();
        ht.CheckCodes(true);
        ht.CalculateDecodeAccelerators();
        ht.CheckDecodeAccelerators(true);
        var x = ht.DecodeBits(121, out var decodedBitsCount);
        Console.WriteLine($"x = {x}, decodedBitsCount = {decodedBitsCount}");
    }
}
