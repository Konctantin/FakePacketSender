using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.Ink;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FakePacketSender.FakePacket
{
    public class FakePacket : BitStreamWriter
    {
        public unsafe delegate void Send2(CDataStore* packet);

        public int Opcode { get; private set; }

        public FakePacket(int opcode)
            : base()
        {
            this.Opcode = opcode;
            this.WriteReverse((uint)opcode, 32);
        }

        public void WriteBits(uint value, int count)
        {
            this.Write(value, count);
        }

        public void WriteInt32(int value)
        {
            this.Buffer.AddRange(BitConverter.GetBytes(value));
            this.Flush();
        }

        public void WriteFloat(float value)
        {
            this.Buffer.AddRange(BitConverter.GetBytes(value));
            this.Flush();
        }

        public void Send_2()
        {
            Console.WriteLine("Send packet: 0x{0:X8}", Opcode);
        }

        public unsafe void Send()
        {
            fixed (byte* pointer = this.Buffer.ToArray())
            {
                var packet = new CDataStore(IntPtr.Zero, new IntPtr(pointer), this.Buffer.Count);
                Send2 sendTo = (Send2)Marshal.GetDelegateForFunctionPointer(IntPtr.Zero, typeof(Send2));
                sendTo(&packet);
                Debug.WriteLine("Send2 Opcode: {0}, Size: {1}", Opcode, this.Buffer.Count);
            }
        }

        public static FakePacket CreateFakePacket(int opcode)
        {
            return new FakePacket(opcode);
        }
    }
}
