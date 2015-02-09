﻿namespace ICSharpCode.SharpZipLib.Zip.Compression
{
    using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
    using System;

    internal class InflaterDynHeader
    {
        private static readonly int[] BL_ORDER = new int[] { 
            0x10, 0x11, 0x12, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 
            14, 1, 15
         };
        private byte[] blLens;
        private const int BLLENS = 3;
        private int blnum;
        private const int BLNUM = 2;
        private InflaterHuffmanTree blTree;
        private int dnum;
        private const int DNUM = 1;
        private byte lastLen;
        private const int LENS = 4;
        private byte[] litdistLens;
        private int lnum;
        private const int LNUM = 0;
        private int mode;
        private int num;
        private int ptr;
        private static readonly int[] repBits = new int[] { 2, 3, 7 };
        private static readonly int[] repMin = new int[] { 3, 3, 11 };
        private const int REPS = 5;
        private int repSymbol;

        public InflaterHuffmanTree BuildDistTree()
        {
            byte[] destinationArray = new byte[this.dnum];
            Array.Copy(this.litdistLens, this.lnum, destinationArray, 0, this.dnum);
            return new InflaterHuffmanTree(destinationArray);
        }

        public InflaterHuffmanTree BuildLitLenTree()
        {
            byte[] destinationArray = new byte[this.lnum];
            Array.Copy(this.litdistLens, 0, destinationArray, 0, this.lnum);
            return new InflaterHuffmanTree(destinationArray);
        }

        public bool Decode(StreamManipulator input)
        {
            int num3;
            int num5;
        Label_0000:
            switch (this.mode)
            {
                case 0:
                    this.lnum = input.PeekBits(5);
                    if (this.lnum >= 0)
                    {
                        this.lnum += 0x101;
                        input.DropBits(5);
                        this.mode = 1;
                        break;
                    }
                    return false;

                case 1:
                    break;

                case 2:
                    goto Label_00BE;

                case 3:
                    goto Label_0149;

                case 4:
                    goto Label_01B8;

                case 5:
                    goto Label_020A;

                default:
                    goto Label_0000;
            }
            this.dnum = input.PeekBits(5);
            if (this.dnum < 0)
            {
                return false;
            }
            this.dnum++;
            input.DropBits(5);
            this.num = this.lnum + this.dnum;
            this.litdistLens = new byte[this.num];
            this.mode = 2;
        Label_00BE:
            this.blnum = input.PeekBits(4);
            if (this.blnum < 0)
            {
                return false;
            }
            this.blnum += 4;
            input.DropBits(4);
            this.blLens = new byte[0x13];
            this.ptr = 0;
            this.mode = 3;
        Label_0149:
            while (this.ptr < this.blnum)
            {
                int num2 = input.PeekBits(3);
                if (num2 < 0)
                {
                    return false;
                }
                input.DropBits(3);
                this.blLens[BL_ORDER[this.ptr]] = (byte) num2;
                this.ptr++;
            }
            this.blTree = new InflaterHuffmanTree(this.blLens);
            this.blLens = null;
            this.ptr = 0;
            this.mode = 4;
        Label_01B8:
            while (((num3 = this.blTree.GetSymbol(input)) & -16) == 0)
            {
                this.litdistLens[this.ptr++] = this.lastLen = (byte) num3;
                if (this.ptr == this.num)
                {
                    return true;
                }
            }
            if (num3 < 0)
            {
                return false;
            }
            if (num3 >= 0x11)
            {
                this.lastLen = 0;
            }
            else if (this.ptr == 0)
            {
                throw new Exception();
            }
            this.repSymbol = num3 - 0x10;
            this.mode = 5;
        Label_020A:
            num5 = repBits[this.repSymbol];
            int num6 = input.PeekBits(num5);
            if (num6 < 0)
            {
                return false;
            }
            input.DropBits(num5);
            num6 += repMin[this.repSymbol];
            if ((this.ptr + num6) > this.num)
            {
                throw new Exception();
            }
            while (num6-- > 0)
            {
                this.litdistLens[this.ptr++] = this.lastLen;
            }
            if (this.ptr == this.num)
            {
                return true;
            }
            this.mode = 4;
            goto Label_0000;
        }
    }
}

