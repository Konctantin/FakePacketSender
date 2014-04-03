using System;
using System.Runtime.InteropServices;

namespace FakePacketSender.Inject
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ManagedParams
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public readonly string DllName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public readonly string TypeName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public readonly string MethodName;

        public ManagedParams(string mDll, string mType, string mFunc)
        {
            DllName  = mDll;
            TypeName = mType;
            MethodName = mFunc;
        }
    }
}
