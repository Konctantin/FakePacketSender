using System;
using System.Linq;
using MS.Internal.Ink;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;

namespace FakePacketSender.FakePacket
{
    public class FakePacket : BitStreamWriter
    {
        // DWORD __stdcall ClientConnection::Send(CDataStore*);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate uint Send2_x32(IntPtr packet);

        [UnmanagedFunctionPointer(CallingConvention.FastCall)]
        delegate uint Send2_x64(IntPtr packet);

        public int Opcode { get; private set; }

        private Process Process;
        private Send2_x32 Send2Func;
        int m_Read = 0;

        public FakePacket(int sendFunctionOffset, int opcode)
        {
            if (sendFunctionOffset == 0)
                throw new ArgumentNullException("Send function offset is empty!");

            if (opcode == 0)
                throw new ArgumentNullException("Opcode must be not 0!");

            Process = Process.GetCurrentProcess();

            // not debug ui mode
            if (!Assembly.GetAssembly(typeof(FakePacket)).Location.Equals(Process.MainModule.FileName))
            {
                Send2Func = Marshal.GetDelegateForFunctionPointer(
                    IntPtr.Add(Process.MainModule.BaseAddress, sendFunctionOffset),
                    typeof(Send2_x32)) as Send2_x32;
            }

            Opcode = opcode;

            Clear();
        }

        public void Clear()
        {
            Buffer.Clear();

            // write header data
            if (Process.MainModule.FileVersionInfo.FilePrivatePart >= 21336)
            {
                WriteInt32(0);
                m_Read = sizeof(int);
                WriteInt16((ushort)Opcode);
            }
            else
            {
                WriteInt32(Opcode);
            }
            Flush();
        }

        public void WriteBits(uint value, int count)
        {
            Write(value, count);
        }

        public void WriteInt16(ushort value)
        {
            Flush();
            Buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void WriteInt32(int value)
        {
            Flush();
            Buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void WriteInt64(long value)
        {
            Flush();
            Buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void WriteFloat(float value)
        {
            Flush();
            Buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void WriteBytes(params byte[] bytes)
        {
            Flush();
            Buffer.AddRange(bytes);
        }

        public void FillBytes(byte value, int count)
        {
            var bytes = new byte[count];
            for (int i = 0; i < count; ++i)
                bytes[i] = value;
            Flush();
            Buffer.AddRange(bytes);
        }

        public string Dump() => string.Join(" ", Buffer.Select(n => n.ToString("X02")));

        public unsafe void Send()
        {
            var byteBuffer = Buffer.ToArray();
            fixed (byte* bytes = byteBuffer)
            {
                var packet = new CDataStore(bytes, byteBuffer.Length, m_Read);

                var packetLen = Marshal.SizeOf(typeof(CDataStore));
                var packetPtr = Marshal.AllocHGlobal(packetLen);

                Marshal.StructureToPtr(packet, packetPtr, true);

                try
                {
                    if (Send2Func == null)
                    {
                        Console.WriteLine(".. Fake [Send2] ..");
                    }
                    else
                    {
                        Send2Func(packetPtr);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    Marshal.FreeHGlobal(packetPtr);
                }
            }
        }

        public static FakePacket CreateFakePacket(int sendFunctionOffset, int opcode)
        {
            return new FakePacket(sendFunctionOffset, opcode);
        }
    }
}