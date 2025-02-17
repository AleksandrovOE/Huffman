using System;

namespace Huffman;

public enum Bit:byte { b0, b1 }; // бит

public class HuffmanTree
{ // дерево Хаффмана
    public const byte ByteBitSize = 8;
    public const short TreeSize = 2 * HalfTreeSize;
    public const short HalfTreeSize = byte.MaxValue + 1;
    public const ushort MaxCodeLengthInBits = byte.MaxValue;
    public const ushort MaxCodeLengthInBytes = (MaxCodeLengthInBits + 7) / 8;

    public record struct Node(short UpLeftNodeNum, short UpRightNodeNum) // узел для дерева Хаффмана
    {
        public static readonly Node Empty = new(-1, -1);
        public override string ToString() => $"({UpLeftNodeNum}, {UpRightNodeNum})";
    } // узел для дерева Хаффмана
    public struct TreeStructure 
    { // структура дерева Хаффмана
        public readonly Node[] Nodes;
        public short Root;
        public Node this[short i] { get => Nodes[i - HalfTreeSize]; set { Nodes[i - HalfTreeSize] = value; } }

        public TreeStructure() { Root = -1; Nodes = new Node[HalfTreeSize]; }
        public bool Assign(Node[] treeNodes, short root)
        {
            Array.Copy(treeNodes, 0, Nodes, 0, root - HalfTreeSize + 1);
            Root = root;
            return CheckTree();
        }
        public bool CheckTree(bool throwException = false)
        {
            if (Root < HalfTreeSize || TreeSize <= Root)
            {
                if (throwException) throw new Exception($"Корень за пределами допустимого диапазона {Root}");
                return false;
            }
            if (Root > HalfTreeSize)
            {
                var NodesCnt = new short[TreeSize];
                for (var i = Root; i >= HalfTreeSize; i--)
                {
                    var n = this[i];
                    if (n.UpLeftNodeNum < 0 || TreeSize <= n.UpLeftNodeNum
                        ||
                        n.UpRightNodeNum < 0 || TreeSize <= n.UpRightNodeNum
                       )
                    {
                        if (throwException) throw new Exception($"Ссылки за пределами допустимого диапазона {i}->{n}");
                        return false;
                    }

                    if (n.UpLeftNodeNum >= i || n.UpRightNodeNum >= i)
                    {
                        if (throwException) throw new Exception($"Некорректные ссылки Up в узле {i}->{n}");
                        return false;
                    }
                    if (++NodesCnt[n.UpLeftNodeNum] > 1 || ++NodesCnt[n.UpRightNodeNum] > 1)
                    {
                        if (throwException) throw new Exception($"Повторная ссылка в узле {i}->{n}");
                        return false;
                    }
                }
            }
            return true;
        }
    } // структура дерева Хаффмана
    record struct NodeData(ulong Count, short NextNodeNum) // доп.данные для дерева Хаффмана
    {
        public static readonly NodeData Empty = new(0, -1);
    } // доп.данные для дерева Хаффмана
    public struct Code
    { // битовый код символа(байта) Хаффмана
        public byte BitLength { get; private set; }
        public readonly byte[] Bits;
        public byte this[int i] => Bits[i];
        public static Code Empty = new();
        public Code() => Bits = new byte[MaxCodeLengthInBytes];
        public Code(byte BitLength, byte[] Bytes) : this()
        {
            this.BitLength = BitLength;
            Array.Copy(Bytes, this.Bits, UsedByteCount);
        }
        public void MakeEmpty() { BitLength = 0; Bits[0] = 0; }
        public void CopyFrom(Code code)
        {
            BitLength = code.BitLength;
            Array.Copy(code.Bits, Bits, UsedByteCount);
        }
        public bool EqualsTo(Code code)
        {
            if (this.BitLength != code.BitLength) return false;
            if (this.BitLength == 0) return false; // пустые коды НЕ равны
            var usedByteCount = UsedByteCount;
            for (var i = 0; i < usedByteCount; i++)
                if (this.Bits[i] != code.Bits[i]) return false;
            return true;
        }
        public void AddBit(Bit bit)
        {
            if (bit == Bit.b1) Bits[ByteNum] |= (byte)(1 << BitNum);
            BitLength++;
        }
        public void DelBit(Bit bit)
        {
            BitLength--;
            if (bit == Bit.b1) Bits[ByteNum] ^= (byte)(1 << BitNum);
        }

        public int UsedByteCount { get => (BitLength + 7) / 8; }
        public int ByteNum { get => BitLength / 8; }
        public int BitNum { get => BitLength % 8; }
    } // битовый код символа(байта) Хаффмана
    public struct DecodeAccelerator
    { // ускоритель декодировки байта для Хаффмана
        public readonly short NodeNum;
        public readonly byte BitLength;
        public static readonly DecodeAccelerator Empty = new(-1, 0);
        public DecodeAccelerator(short NodeNum, byte BitLength) { this.NodeNum = NodeNum; this.BitLength = BitLength; }
    } // ускоритель декодировки байта для Хаффмана

    TreeStructure Tree;
    readonly NodeData[] NodesData;
    readonly short[] Indexes;
    readonly uint[] Cnts;
    public readonly Code[] Codes;
    readonly DecodeAccelerator[] DecodeAccelerators;

    public void Rebuild(uint[] Counters)
    {   // инициализация массивов
        if (Counters.Length != HalfTreeSize) throw new ArgumentException("Неверная длина массива Counters");
        Array.Copy(Counters, Cnts, HalfTreeSize);
        var i = 0; foreach (var c in Cnts) NodesData[i++].Count = c;
        var n = HalfTreeSize; while( n-- > 0) Indexes[n] = n;
        // сортировка частот 
        Array.Sort(Cnts, Indexes);
        // поиск первой ненулевой частоты в базовой таблице частот
        var x = Array.BinarySearch(Cnts, 1u); if (x < 0) x = ~x; else while (x > 0 && Cnts[x - 1] == 1u) x--;
        var iFirst = (short)Math.Min(x, HalfTreeSize - 2); // если меньше двух символов - добавляем фиктивные
        // добавление первого нового узла из базовой таблицы частот
        var nFirst = HalfTreeSize; var nLast = HalfTreeSize;
        AddNode(nFirst, Indexes[iFirst++], Indexes[iFirst++]);
        // слияние базовой таблицы частот и новых узлов
        while (iFirst < HalfTreeSize) // пока не пуста базовая таблица частот
        {
            var left  = GetSmallestNode();
            var right = GetSmallestNode();
            AddNode(++nLast, left, right);
        }
        // слияние остатка новых узлов
        while (nFirst < nLast) AddNode(++nLast, nFirst++, nFirst++);
        // запоминаем корень дерева
        Tree.Root = nLast;

        short GetSmallestNode()
        {
            if (iFirst >= HalfTreeSize) return nFirst++;
            var ni = Indexes[iFirst];
            if (nFirst <= nLast && NodesData[nFirst].Count < NodesData[ni].Count) return nFirst++;
            iFirst++;
            return ni;
        }
    }

    void AddNode(short n, short l, short r)
    {
        Tree[n] = new(l, r);
        NodesData[l].NextNodeNum = NodesData[r].NextNodeNum = n;
        NodesData[n].Count = NodesData[l].Count + NodesData[r].Count;
    }

    public void CalculateCodes()
    {
        var code = new Code(); // для вычисления текущего кода
        var i = Codes.Length; while (i-- > 0) Codes[i].MakeEmpty(); // зануляем массив кодов
        var node = Tree[Tree.Root];
        Calculate(node.UpLeftNodeNum, Bit.b0); // вычисляем для левой ветки
        Calculate(node.UpRightNodeNum, Bit.b1); // вычисляем для правой ветки
        ///////////////////////////////////////
        void Calculate(short nodeNum, Bit bit) // рекурсивный вычислитель
        {   
            code.AddBit(bit);
            if (nodeNum < HalfTreeSize) 
                Codes[nodeNum].CopyFrom(code); // завершено вычисление кода 
            else
            { // продолжаем вычисление...
                Calculate(Tree[nodeNum].UpLeftNodeNum, Bit.b0);
                Calculate(Tree[nodeNum].UpRightNodeNum, Bit.b1);
            }
            code.DelBit(bit);
        }
    }

    public void CalculateDecodeAccelerators()
    { /* УСКОРЕННАЯ ТАБЛИЧНАЯ ДЕКОДИРОВКА
          Идея алгоритма состоит в том, что большая часть кодов Хаффмана короткие
        коды (<8 бит), иначе нам не получить сокращение размера данных при упаковке,
        и только редкие коды длиннее 8 бит. Поэтому, значительно ускорив декодировку
        коротких кодов, мы можем существенно ускорить декодировку в целом.
          Ускорить декодировку коротких кодов можно с помощью предварительного
        создания декодировочной таблицы для 8 бит кода(байта). В этом случае мы будем
        декодировать короткие коды (и начало длинных кодов) за одно обращение к
        таблице, а остаток кода можно декодировать проходом по дереву.
      */
        Array.Fill(DecodeAccelerators, DecodeAccelerator.Empty);
        var nodeNum = (short)0;
        foreach (var c in Codes)
        {
            if (c.BitLength == 0) continue;
            var bitSize = Math.Min(c.BitLength, ByteBitSize);
            var codeByte = c[0];
            var da = DecodeAccelerators[codeByte] = new(nodeNum, bitSize);
            var b = 1 << (ByteBitSize - bitSize);
            while (--b > 0) DecodeAccelerators[(b << c.BitLength) | codeByte] = da;
            nodeNum++;
        }
    }

    public ulong GetCompressedSizeInBits()
    {
        var s = 0ul;
        for (var i = 0; i < HalfTreeSize; i++) s += NodesData[i].Count * Codes[i].BitLength;
        return s;
    }

    public int GetCompressedSizeInBytes() => (int)((GetCompressedSizeInBits() + 7) / 8);

    public short DecodeBits(byte @byte, out byte decodedBitsCount, short startNodeNum = TreeSize)
    {
        if (startNodeNum == TreeSize)
          { var da = DecodeAccelerators[@byte]; decodedBitsCount = da.BitLength; return da.NodeNum; }
        var cnt = (byte)0; 
        while (cnt < ByteBitSize && startNodeNum >= HalfTreeSize) 
            startNodeNum = (@byte & (1 << cnt++)) > 0 ? Tree[startNodeNum].UpRightNodeNum 
                                                      : Tree[startNodeNum].UpLeftNodeNum;
        decodedBitsCount = cnt;
        return startNodeNum;
    }

    public short DecodeBitsNoAcc(byte @byte, out byte decodedBitsCount, short startNodeNum = TreeSize)
    {
        if (startNodeNum == TreeSize) startNodeNum = Tree.Root;
        var cnt = (byte)0;
        while (cnt < ByteBitSize && startNodeNum >= HalfTreeSize)
            startNodeNum = (@byte & (1 << cnt++)) > 0 ? Tree[startNodeNum].UpRightNodeNum
                                                      : Tree[startNodeNum].UpLeftNodeNum;
        decodedBitsCount = cnt;
        return startNodeNum;
    }

    public bool CheckTree(bool throwException = false) => Tree.CheckTree(throwException);

    public bool CheckCodes(bool throwException = false)
    {
        for (var i = 0; i < HalfTreeSize; i++)
            for (var j = i + 1; j < HalfTreeSize; j++)
                if (Codes[i].EqualsTo(Codes[j]))
                {
                    if (throwException) throw new Exception($"Коды {i} и {j} совпадают");
                    return false;
                }
        return true;
    }

    public bool CheckDecodeAccelerators(bool throwException = false)
    {
        for (var i = 0; i < HalfTreeSize; i++)
        {
            var dai = DecodeAccelerators[i];
            if (dai.BitLength == 0 || dai.NodeNum == -1)
            {
                if (throwException) throw new Exception($"Неинициализированный ускоритель {i}.");
                return false;
            }
            for (var j = i + 1; j < HalfTreeSize; j++)
            {
                var daj = DecodeAccelerators[j];
                if (dai.NodeNum == daj.NodeNum && dai.BitLength != daj.BitLength)
                {
                    if (throwException) throw new Exception($"Символы {i} и {j} совпадают, но длина кода разная.");
                    return false;
                }
                if (dai.NodeNum == daj.NodeNum 
                   && (i & ((1 << dai.BitLength) -1 )) != (j & ((1 << daj.BitLength) - 1)) )
                {
                    if (throwException) throw new Exception($"Символы {i} и {j} совпадают, но коды разные.");
                    return false;
                }
            }
        }
        return true;
    }

    public HuffmanTree()
    {
        NodesData = new NodeData[TreeSize];
        Tree = new();
        Indexes = new short[HalfTreeSize]; 
        Cnts = new uint[HalfTreeSize];
        Codes = new Code[HalfTreeSize]; var i = Codes.Length; while (i-- > 0) Codes[i] = new();
        DecodeAccelerators = new DecodeAccelerator[HalfTreeSize];
    }
} // дерево Хаффмана
