using System;

namespace Huffman;

public enum Operation: byte { Read, Write };

public class BitBuffer
{ // для чтения и записи буфера порциями бит не кратными 8 (байту).
    byte[]? Buffer;
    Operation CurrentOperation = Operation.Read;
    int CurrentByte; // текущий байт. При записи - свободный (частично). При чтении - непрочитанный (частично).
    int CurrentBit; // текущий свободный бит в байте. Только для записи.
    public long TotalBitsCount { get; private set; } // всего битов
    public int TotalBytesCount { get => (int)((TotalBitsCount + 7) / 8); } // всего байтов
    ushort BuffBits; // буфер чтения байта. Только для чтения
    int BuffBitsLength; // количество бит в буфере чтения. Только для чтения

    public BitBuffer(byte[] Buffer) => Reset(Buffer, Operation.Read);
    public BitBuffer() => Reset(null);

    public void Reset(byte[]? Buffer, Operation operation = Operation.Read) // подключить байтовый буфер и установит текущую позицию на начало
    { 
        this.Buffer = Buffer; 
        CurrentByte = 0; CurrentBit = 0; 
        TotalBitsCount = 0; 
        BuffBits = 0; BuffBitsLength = 0;
        CurrentOperation = operation;
    }

    public bool AddBits(int bitsCount, byte[] bits) // записать порцию бит в буфер
    {
        if (CurrentOperation != Operation.Read) throw new Exception("Буфер должен быть в состоянии Read");
        var fullBitCount = bitsCount + CurrentBit;
        var fullByteCount = (fullBitCount + 7) >> 3;
        if ( (CurrentByte + fullByteCount) > Buffer?.Length) return false;
        TotalBitsCount += bitsCount;
        if (CurrentBit == 0)
        {
            Array.Copy(bits, 0, Buffer, CurrentByte, fullByteCount);
            CurrentByte += (fullBitCount >> 3);
        }
        else
        {
            var bitShift = (8 - CurrentBit);
            var byteCount = (bitsCount + 7) >> 3;
            var buf = (ushort)Buffer[CurrentByte];
            // копирование полных байт в выходной буфер 
            for (var i = 0; i < byteCount; i++)
            {
                buf = (ushort)((buf << 8) | (bits[i] << bitShift));
                Buffer[CurrentByte++] = (byte)(buf >> 8);
            }
            if (byteCount < fullByteCount) Buffer[CurrentByte] = (byte)buf;
        }
        CurrentBit = fullBitCount & 0x111;
        return true;
    }

    public bool GetByte(byte bitsCount, out byte @byte) // пропустить bitsCount бит и вернуть следующую за ними порцию 8 бит (байт)
    {
        if (CurrentOperation != Operation.Write) throw new Exception("Буфер должен быть в состоянии Write");
        BuffBitsLength -= bitsCount; TotalBitsCount += bitsCount;
        if (BuffBitsLength < 8)
        {
            if (CurrentByte < Buffer?.Length)
            {
                BuffBits = (ushort)((BuffBits << bitsCount) | Buffer[CurrentByte++]);
                BuffBitsLength += 8;
            }
            if (BuffBitsLength == 0) { @byte = 0; return false; }
        }
        @byte = (byte)(BuffBits >> 8);
        return true;
    }
} // для чтения/записи буфера порциями бит не кратными 8 (байту).
