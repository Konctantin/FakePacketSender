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

        public CDataStore(void* vTable, byte* buffer, int size)
        {
            this.vTable = vTable;
            this.Buffer = buffer;
            this.mBase  = 0;
            this.alloc  = 0;
            this.size   = size;
            this.read   = 0;
        }
    }
}
