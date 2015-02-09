﻿namespace Ionic.Zip
{
    using System;
    using System.IO;

    public class CRC32
    {
        private uint _RunningCrc32Result = uint.MaxValue;
        private long _TotalBytesRead;
        private const int BUFFER_SIZE = 0x2000;
        private static uint[] crc32Table;

        static CRC32()
        {
            uint num = 0xedb88320;
            crc32Table = new uint[0x100];
            for (uint i = 0; i < 0x100; i++)
            {
                uint num3 = i;
                for (uint j = 8; j > 0; j--)
                {
                    if ((num3 & 1) == 1)
                    {
                        num3 = (num3 >> 1) ^ num;
                    }
                    else
                    {
                        num3 = num3 >> 1;
                    }
                }
                crc32Table[i] = num3;
            }
        }

        public int ComputeCrc32(int W, byte B)
        {
            return this.ComputeCrc32((uint) W, B);
        }

        internal int ComputeCrc32(uint W, byte B)
        {
            return (int) (crc32Table[(int) ((IntPtr) ((W ^ B) & 0xff))] ^ (W >> 8));
        }

        public int GetCrc32(Stream input)
        {
            return this.GetCrc32AndCopy(input, null);
        }

        public int GetCrc32AndCopy(Stream input, Stream output)
        {
            if (input == null)
            {
                throw new ZipException("bad input.", new ArgumentException("The input stream must not be null.", "input"));
            }
            byte[] buffer = new byte[0x2000];
            int count = 0x2000;
            this._TotalBytesRead = 0L;
            int num2 = input.Read(buffer, 0, count);
            if (output != null)
            {
                output.Write(buffer, 0, num2);
            }
            this._TotalBytesRead += num2;
            while (num2 > 0)
            {
                this.SlurpBlock(buffer, 0, num2);
                num2 = input.Read(buffer, 0, count);
                if (output != null)
                {
                    output.Write(buffer, 0, num2);
                }
                this._TotalBytesRead += num2;
            }
            return (int) ~this._RunningCrc32Result;
        }

        public void SlurpBlock(byte[] block, int offset, int count)
        {
            if (block == null)
            {
                throw new ZipException("Bad buffer.", new ArgumentException("The data buffer must not be null.", "block"));
            }
            for (int i = 0; i < count; i++)
            {
                int index = offset + i;
                this._RunningCrc32Result = (this._RunningCrc32Result >> 8) ^ crc32Table[(int) ((IntPtr) (block[index] ^ (this._RunningCrc32Result & 0xff)))];
            }
            this._TotalBytesRead += count;
        }

        public int Crc32Result
        {
            get
            {
                return (int) ~this._RunningCrc32Result;
            }
        }

        public long TotalBytesRead
        {
            get
            {
                return this._TotalBytesRead;
            }
        }
    }
}

