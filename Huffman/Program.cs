namespace Huffman;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine($"Hello, World! {1<<2}");
        var ht = new HuffmanTree();
        var a = new uint[256];
        var i = 256;
        while (i-- > 0)
        {
            a[i] = 20 < i && i < 100 ? (uint)Random.Shared.Next(10) : (uint)Random.Shared.Next(100000);
        }
        ht.Rebuild(a);
        ht.CheckTree(true);
        ht.CalculateCodes();
        ht.CheckCodes(true);
        ht.CalculateDecodeAccelerators();
        ht.CheckDecodeAccelerators(true);
        var bb = (byte)121;
        Console.WriteLine($"bb = {bb:B8}");
        var x = ht.DecodeBits(bb, out var decodedBitsCount);
        Console.WriteLine($"code[x] = {ht.Codes[x].Bits[0]:B8}, lenght = {ht.Codes[x].BitLength}");
        Console.WriteLine($"x = {x}, decodedBitsCount = {decodedBitsCount}");
        x = ht.DecodeBitsNoAcc(bb, out decodedBitsCount);
        Console.WriteLine($"code[x] = {ht.Codes[x].Bits[0]:B8}, lenght = {ht.Codes[x].BitLength}");
        Console.WriteLine($"x = {x}, decodedBitsCount = {decodedBitsCount}");


        Huffman hf = new(10000);
        var dataBuffer = new byte[9000];
        var n = dataBuffer.Length;
        while (n-- > 0)
        {
            var b = (byte)Random.Shared.Next(256);
            dataBuffer[n] = 12 < b && b < 100 ? (byte)90 : b; 
        }
        var res = hf.Encode(dataBuffer, dataBuffer.Length);
        Console.WriteLine($"{res} размер = {hf.CountByteInBuffer} байт, {hf.CountBitsInBuffer} бит");

        var bitsCount = hf.CountBitsInBuffer;
        var EncodedBuffer = (byte[])hf.Buffer.Clone();
        var resD = hf.Decode(EncodedBuffer, bitsCount);
    }
}
