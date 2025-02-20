using System;

namespace Huffman;

public enum Operation: byte { Read, Write };

public class BitBuffer
{ // для чтения и записи буфера порциями бит не кратными 8 (байту).
    byte[]? Buffer;
    int BufferSize;
    Operation Operation = Operation.Read;
    int CurrentByte; // текущий байт. При записи - свободный (частично). При чтении - непрочитанный (частично).
    int CurrentBit; // текущий свободный бит в байте. Только для записи.
    public long TotalBitsCount { get; private set; } // всего битов
    public int TotalBytesCount { get => (int)((TotalBitsCount + 7) / 8); } // всего байтов
    ushort BuffBits; // буфер чтения байта. Только для чтения
    int BuffBitsLength; // количество бит в буфере чтения. Только для чтения

    public BitBuffer(byte[] Buffer, Operation operation = Operation.Read) => Reset(Buffer, Buffer.Length, operation);
    public BitBuffer() => Reset(null, 0);

    public void Reset(byte[]? Buffer, int size, Operation operation = Operation.Read) // подключить байтовый буфер и установит текущую позицию на начало
    {
        if (size < 0 || Buffer?.Length < size) throw new Exception("Некорректный размер size={size}");
        this.Buffer = Buffer;
        BufferSize = size;
        CurrentByte = 0; CurrentBit = 0; 
        TotalBitsCount = 0; 
        BuffBits = 0; BuffBitsLength = 0;
        Operation = operation;
    }

    public bool AddBits(byte bitsCount, byte[] bits) // записать порцию бит в буфер
    {
        if (Operation != Operation.Write) throw new Exception("Буфер должен быть в состоянии Write");
        var fullBitCount = bitsCount + CurrentBit;
        var fullByteCount = (fullBitCount + 7) >> 3;
        if ( (CurrentByte + fullByteCount) > Buffer?.Length) return false;
        TotalBitsCount += bitsCount;
        if (CurrentBit == 0)
        { // без сдвига
            Array.Copy(bits, 0, Buffer, CurrentByte, fullByteCount);
            CurrentByte += (fullBitCount >> 3);
        }
        else
        { // со сдвигом
            var shift = (8 - CurrentBit);
            var byteCount = (bitsCount + 7) >> 3;
            var buf = (ushort)Buffer[CurrentByte];
            // копирование полных байт в выходной буфер 
            for (var i = 0; i < byteCount; i++)
            {
                buf = (ushort)((buf << 8) | (bits[i] << shift));
                Buffer[CurrentByte++] = (byte)(buf >> 8);
            }
            if (byteCount < fullByteCount) Buffer[CurrentByte] = (byte)buf;
        }
        CurrentBit = fullBitCount & 0x111;
        return true;
    }

    public bool Get8Bits(byte bitsCount, out byte _8bits) // пропустить bitsCount бит и вернуть следующую за ними порцию 8 бит (байт)
    {
        if (Operation != Operation.Read) throw new Exception("Буфер должен быть в состоянии Read");
        if (bitsCount > 8) throw new Exception("Значение bitsCount = {bitsCount}");
        if (bitsCount > BuffBitsLength) throw new Exception("Значение bitsCount = {bitsCount} не согласовано с длиной буфера {BuffBitsLength}");
        BuffBitsLength -= bitsCount; TotalBitsCount += bitsCount;
        BuffBits <<= bitsCount;
        if (BuffBitsLength < 8) 
            if (CurrentByte < Buffer?.Length)
            {
                var shift = (8 - BuffBitsLength);
                BuffBits = (ushort)( ((BuffBits >> shift) | Buffer[CurrentByte++]) << shift );
                BuffBitsLength += 8;
            }
            else if (BuffBitsLength == 0) { _8bits = 0; return false; }
        _8bits = (byte)(BuffBits >> 8);
        return true;
    }
} // для чтения/записи буфера порциями бит не кратными 8 (байту).
