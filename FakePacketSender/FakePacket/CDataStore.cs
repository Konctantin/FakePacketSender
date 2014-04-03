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
        public IntPtr vTable;
        public IntPtr Buffer;
        public int mBase;
        public int alloc;
        public int size;
        public int read;

        public CDataStore(IntPtr vTable, IntPtr buffer, int size)
        {
            this.vTable = vTable;
            this.Buffer = IntPtr.Zero;
            this.mBase  = 0;
            this.alloc  = 0;
            this.size   = size;
            this.read   = 0;
        }
        public CDataStore(IntPtr vTable)
            : this(vTable, IntPtr.Zero, 0)
        {
        }
    }
}
