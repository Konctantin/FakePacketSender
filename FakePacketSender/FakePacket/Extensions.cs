using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakePacketSender.FakePacket
{
    public class Extensions
    {
        public static int Bit_Or(int input, params int[] values)
        {
            foreach (var val in values)
                input |= val;
            return input;
        }

        public static int Bit_Xor(int input, int value)
        {
            return input ^ value;
        }

        public static int Bit_And(int input, int value)
        {
            return input & value;
        }

        public static int Bit_Not(int input, int value)
        {
            return input & ~value;
        }

        public static int Bit_Lsh(int input, int value)
        {
            return input << value;
        }

        public static int Bit_Rsh(int input, int value)
        {
            return input >> value;
        }
    }
}
