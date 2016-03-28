using System;
using System.Collections.Generic;

namespace MS.Internal.Ink
{
    public class BitStreamWriter
    {
        public List<byte> Buffer { get; private set; }

        private int remaining;

        public BitStreamWriter()
        {
            Buffer = new List<byte>();
        }

        public void Flush()
        {
            remaining = 0;
        }

        public void Write(uint bits, int countOfBits)
        {
            if (countOfBits <= 0 || countOfBits > 32)
                throw new ArgumentOutOfRangeException("countOfBits", countOfBits, "CountOfBitsOutOfRange");

            int i   = countOfBits / 8;
            int num = countOfBits % 8;

            while (i >= 0)
            {
                byte bits2 = (byte)(bits >> i * 8);
                if (num > 0)
                    Write(bits2, num);

                if (i > 0)
                    num = 8;

                i--;
            }
        }

        public void WriteReverse(uint bits, int countOfBits)
        {
            if (countOfBits <= 0 || countOfBits > 32)
                throw new ArgumentOutOfRangeException("countOfBits", countOfBits, "CountOfBitsOutOfRange");

            int num  = countOfBits / 8;
            int num2 = countOfBits % 8;

            if (num2 > 0)
                num++;

            for (int i = 0; i < num; i++)
            {
                byte bits2 = (byte)(bits >> i * 8);
                Write(bits2, 8);
            }
        }

        public void Write(byte bits, int countOfBits)
        {
            if (countOfBits <= 0 || countOfBits > 8)
                throw new ArgumentOutOfRangeException("countOfBits", countOfBits, "CountOfBitsOutOfRange");

            if (remaining > 0)
            {
                byte b = Buffer[Buffer.Count - 1];

                if (countOfBits > remaining)
                    b |= (byte)(((int)bits & 255 >> 8 - countOfBits) >> countOfBits - remaining);
                else
                    b |= (byte)(((int)bits & 255 >> 8 - countOfBits) << remaining - countOfBits);

                Buffer[Buffer.Count - 1] = b;
            }

            if (countOfBits > remaining)
            {
                remaining = 8 - (countOfBits - remaining);
                byte b = (byte)(bits << remaining);
                Buffer.Add(b);
                return;
            }
            remaining -= countOfBits;
        }
    }
}
