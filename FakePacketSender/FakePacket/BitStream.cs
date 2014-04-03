using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WowPacketViewer.Wow.IO
{
    // import from MS.Internal.Ink.BitStreamReader
    public class BitStreamReader
    {
        #region Basic Implementions

        public byte[] Buffer { get; private set; }
        public int Index { get; private set; }

        public int RemaningLength
        {
            get { return Buffer.Length - this.Index; }
        }

        private uint countBits;
        private byte partialByte;
        private int cbitsInPartialByte;

        public bool EndOfStream
        {
            get { return 0u == this.countBits; }
        }

        public BitStreamReader(byte[] buffer)
        {
            this.Buffer = buffer;
            this.countBits = (uint)(buffer.Length * 8);
        }

        public BitStreamReader(byte[] buffer, int startIndex)
        {
            if (startIndex < 0 || startIndex >= buffer.Length)
                throw new ArgumentOutOfRangeException("startIndex");

            this.Buffer = buffer;
            this.Index = startIndex;
            this.countBits = (uint)((buffer.Length - startIndex) * 8);
        }

        public BitStreamReader(byte[] buffer, uint bufferLengthInBits)
            : this(buffer)
        {
            if ((ulong)bufferLengthInBits > (ulong)((long)(buffer.Length * 8)))
                throw new ArgumentOutOfRangeException("bufferLengthInBits", "InvalidBufferLength");

            this.countBits = bufferLengthInBits;
        }

        public void Skip(int length)
        {
            this.Index += length;
        }

        public long ReadUInt64(int countOfBits)
        {
            if (countOfBits > 64 || countOfBits <= 0)
                throw new ArgumentOutOfRangeException("countOfBits", countOfBits, "CountOfBitsOutOfRange");

            long num = 0L;
            while (countOfBits > 0)
            {
                int num2 = 8;
                if (countOfBits < 8)
                    num2 = countOfBits;

                num <<= num2;
                byte b = this.ReadByte(num2);
                num |= (long)((ulong)b);
                countOfBits -= num2;
            }
            return num;
        }

        public ushort ReadUInt16(int countOfBits)
        {
            if (countOfBits > 16 || countOfBits <= 0)
                throw new ArgumentOutOfRangeException("countOfBits", countOfBits, "CountOfBitsOutOfRange");

            ushort num = 0;
            while (countOfBits > 0)
            {
                int num2 = 8;
                if (countOfBits < 8)
                    num2 = countOfBits;

                num = (ushort)(num << num2);
                byte b = this.ReadByte(num2);
                num |= (ushort)b;
                countOfBits -= num2;
            }
            return num;
        }

        public uint ReadUInt16Reverse(int countOfBits)
        {
            if (countOfBits > 16 || countOfBits <= 0)
                throw new ArgumentOutOfRangeException("countOfBits", countOfBits, "CountOfBitsOutOfRange");

            ushort num = 0;
            int num2 = 0;
            while (countOfBits > 0)
            {
                int num3 = 8;
                if (countOfBits < 8)
                    num3 = countOfBits;

                ushort num4 = (ushort)this.ReadByte(num3);
                num4 = (ushort)(num4 << num2 * 8);
                num |= num4;
                num2++;
                countOfBits -= num3;
            }
            return (uint)num;
        }

        public uint ReadUInt32(int countOfBits)
        {
            if (countOfBits > 32 || countOfBits <= 0)
                throw new ArgumentOutOfRangeException("countOfBits", countOfBits, "CountOfBitsOutOfRange");

            uint num = 0u;
            while (countOfBits > 0)
            {
                int num2 = 8;
                if (countOfBits < 8)
                    num2 = countOfBits;

                num <<= num2;
                byte b = this.ReadByte(num2);
                num |= (uint)b;
                countOfBits -= num2;
            }
            return num;
        }

        public uint ReadUInt32Reverse(int countOfBits)
        {
            if (countOfBits > 32 || countOfBits <= 0)
                throw new ArgumentOutOfRangeException("countOfBits", countOfBits, "CountOfBitsOutOfRange");

            uint num = 0u;
            int num2 = 0;
            while (countOfBits > 0)
            {
                int num3 = 8;
                if (countOfBits < 8)
                    num3 = countOfBits;

                uint num4 = (uint)this.ReadByte(num3);
                num4 <<= num2 * 8;
                num |= num4;
                num2++;
                countOfBits -= num3;
            }
            return num;
        }

        public bool ReadBit()
        {
            byte b = this.ReadByte(1);
            return (b & 1) == 1;
        }

        public byte ReadByte(int countOfBits)
        {
            if (this.EndOfStream)
                throw new EndOfStreamException("EndOfStreamReached");

            if (countOfBits > 8 || countOfBits <= 0)
                throw new ArgumentOutOfRangeException("countOfBits", countOfBits, "CountOfBitsOutOfRange");

            if ((long)countOfBits > (long)((ulong)this.countBits))
                throw new ArgumentOutOfRangeException("countOfBits", countOfBits, "CountOfBitsGreatThanRemainingBits");

            this.countBits -= (uint)countOfBits;
            byte b;

            if (this.cbitsInPartialByte >= countOfBits)
            {
                int num = 8 - countOfBits;
                b = (byte)(this.partialByte >> num);
                this.partialByte = (byte)(this.partialByte << countOfBits);
                this.cbitsInPartialByte -= countOfBits;
            }
            else
            {
                byte b2 = this.Buffer[this.Index];
                this.Index++;
                int num2 = 8 - countOfBits;
                b = (byte)(this.partialByte >> num2);
                int num3 = Math.Abs(countOfBits - this.cbitsInPartialByte - 8);
                b |= (byte)(b2 >> num3);
                this.partialByte = (byte)(b2 << countOfBits - this.cbitsInPartialByte);
                this.cbitsInPartialByte = 8 - (countOfBits - this.cbitsInPartialByte);
            }
            return b;
        }

        #endregion

        #region Extensions BinaryReader
        public string ReadCString()
        {
            var bytes = new List<byte>();

            byte b;
            while ((b = ReadByte()) != 0 || this.Index < this.Buffer.Length - 1)  // CDataStore::GetCString calls CanRead too
                bytes.Add(b);

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public byte ReadByte()
        {
            var val = Buffer[Index];
            ++Index;
            return val;
        }

        public byte[] ReadBytes(int count)
        {
            var arr = new byte[count];
            Array.Copy(Buffer, arr, count);
            this.Index += count;
            return arr;
        }

        public short ReadInt16()
        {
            var val = BitConverter.ToInt16(Buffer, Index);
            Index += 2;
            return val;
        }

        public ushort ReadUInt16()
        {
            var val = BitConverter.ToUInt16(Buffer, Index);
            Index += 2;
            return val;
        }

        public int ReadInt32()
        {
            var val = BitConverter.ToInt32(Buffer, Index);
            Index += 4;
            return val;
        }

        public uint ReadUInt32()
        {
            var val = BitConverter.ToUInt32(Buffer, Index);
            Index += 4;
            return val;
        }

        public long ReadInt64()
        {
            var val = BitConverter.ToInt64(Buffer, Index);
            Index += 8;
            return val;
        }

        public ulong ReadUInt64()
        {
            var val = BitConverter.ToUInt64(Buffer, Index);
            Index += 8;
            return val;
        }

        public float ReadFloat()
        {
            var val = BitConverter.ToSingle(Buffer, Index);
            Index += 4;
            return val;
        }

        #endregion

        #region Ex BinaryReader
        public unsafe T Read<T>() where T : struct
        {
            return default(T);
            //if (StructHelper<T>.Size > RemaningLength)
            //    throw new EndOfStreamException();

            //fixed (void* pointer = ReadBytes(StructHelper<T>.Size))
            //{
            //    switch (StructHelper<T>.TypeCode)
            //    {
            //        case TypeCode.Boolean: return (T)(object)(*(byte*)pointer != 0);
            //        case TypeCode.Char:    return (T)(object)*(char*)pointer;
            //        case TypeCode.SByte:   return (T)(object)*(sbyte*)pointer;
            //        case TypeCode.Byte:    return (T)(object)*(byte*)pointer;
            //        case TypeCode.Int16:   return (T)(object)*(short*)pointer;
            //        case TypeCode.UInt16:  return (T)(object)*(ushort*)pointer;
            //        case TypeCode.Int32:   return (T)(object)*(int*)pointer;
            //        case TypeCode.UInt32:  return (T)(object)*(uint*)pointer;
            //        case TypeCode.Int64:   return (T)(object)*(long*)pointer;
            //        case TypeCode.UInt64:  return (T)(object)*(ulong*)pointer;
            //        case TypeCode.Single:  return (T)(object)*(float*)pointer;
            //        case TypeCode.Double:  return (T)(object)*(double*)pointer;

            //        case TypeCode.Object:
            //            return (T)Marshal.PtrToStructure(new IntPtr(pointer), StructHelper<T>.Type);

            //        default: throw new ArgumentOutOfRangeException();
            //    }
            //}
        }

        #endregion

        #region WowReaders

        public DateTime ReadUnixTime()
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(ReadInt32());
        }

        public DateTime ReadGameTime()
        {
            int packedDate = ReadInt32();

            var minute  = packedDate & 0x3F;
            var hour    = (packedDate >> 6)  & 0x1F;
            var day     = (packedDate >> 14) & 0x3F;
            var month   = (packedDate >> 20) & 0xF;
            var year    = (packedDate >> 24) & 0x1F;

            return new DateTime(2000 + year, month, day, hour, minute, 0);
        }

        #endregion
    }
}