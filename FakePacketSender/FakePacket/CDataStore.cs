using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace FakePacketSender.FakePacket
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CDataStore
    {
        public void* vTable;
        public byte* Buffer;
        public int mBase;
        public int alloc;
        public int size;
        public int read;

        public CDataStore(byte* buffer, int size)
        {
            vTable = null;
            Buffer = buffer;
            mBase = 0;
            alloc = -1;
            this.size   = size;
            read = 0;
        }
    }
}
