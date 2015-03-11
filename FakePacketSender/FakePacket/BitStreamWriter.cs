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
            this.Buffer = new List<byte>();
        }

        public void Flush()
        {
            this.remaining = 0;
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
                    this.Write(bits2, num);

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
                this.Write(bits2, 8);
            }
        }

        public void Write(byte bits, int countOfBits)
        {
            if (countOfBits <= 0 || countOfBits > 8)
                throw new ArgumentOutOfRangeException("countOfBits", countOfBits, "CountOfBitsOutOfRange");

            if (this.remaining > 0)
            {
                byte b = this.Buffer[this.Buffer.Count - 1];

                if (countOfBits > this.remaining)
                    b |= (byte)(((int)bits & 255 >> 8 - countOfBits) >> countOfBits - this.remaining);
                else
                    b |= (byte)(((int)bits & 255 >> 8 - countOfBits) << this.remaining - countOfBits);

                this.Buffer[this.Buffer.Count - 1] = b;
            }

            if (countOfBits > this.remaining)
            {
                this.remaining = 8 - (countOfBits - this.remaining);
                byte b = (byte)(bits << this.remaining);
                this.Buffer.Add(b);
                return;
            }
            this.remaining -= countOfBits;
        }
    }
}
