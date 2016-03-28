using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FakePacketSender.Inject
{
    public class ProcessMemory
    {
        #region API

        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr OpenThread(ThreadAccess DesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwThreadId);
        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, FreeType dwFreeType);
        [DllImport("kernel32", SetLastError = true)]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, MemoryProtection flNewProtect, out MemoryProtection lpflOldProtect);
        [DllImport("kernel32", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32", SetLastError = true)]
        public static extern uint SuspendThread(IntPtr thandle);
        [DllImport("kernel32", SetLastError = true)]
        public static extern uint ResumeThread(IntPtr thandle);
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool GetThreadContext(IntPtr thandle, ref CONTEXT context);
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool SetThreadContext(IntPtr thandle, ref CONTEXT context);
        [DllImport("user32")]
        static extern IntPtr GetForegroundWindow();

        #endregion

        public Process Process { get; private set; }

        public ProcessMemory(Process process)
        {
            Process = process;
        }

        public IntPtr Alloc(int size)
        {
            if (size <= 0)
                throw new ArgumentNullException("size");

            var address = VirtualAllocEx(Process.Handle, IntPtr.Zero, size, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);

            if (address == IntPtr.Zero)
                throw new Win32Exception();

            return address;
        }

        public void Free(IntPtr address)
        {
            if (address == IntPtr.Zero)
                throw new ArgumentNullException("address");

            if (!VirtualFreeEx(Process.Handle, address, 0, FreeType.Release))
                throw new Win32Exception();
        }

        public unsafe byte[] ReadBytes(IntPtr address, int count)
        {
            var bytes = new byte[count];
            if(!ReadProcessMemory(Process.Handle, address, bytes, count, IntPtr.Zero))
                throw new Win32Exception();
            return bytes;
        }

        public unsafe T Read<T>(IntPtr address) where T : struct
        {
            var result = new byte[Marshal.SizeOf(typeof(T))];
            ReadProcessMemory(Process.Handle, address, result, result.Length, IntPtr.Zero);
            var handle = GCHandle.Alloc(result, GCHandleType.Pinned);
            T returnObject = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return returnObject;
        }

        public string ReadString(IntPtr addess, int length = 100)
        {
            var result = new byte[length];
            if (!ReadProcessMemory(Process.Handle, addess, result, length, IntPtr.Zero))
                throw new Win32Exception();
            return Encoding.UTF8.GetString(result.TakeWhile(ret => ret != 0).ToArray());
        }

        public IntPtr Write<T>(T value) where T : struct
        {
            var buffer  = new byte[Marshal.SizeOf(value)];
            var hObj    = Marshal.AllocHGlobal(buffer.Length);
            var address = Alloc(buffer.Length);
            if (address == IntPtr.Zero)
                throw new Win32Exception();
            try
            {
                Marshal.StructureToPtr(value, hObj, false);
                Marshal.Copy(hObj, buffer, 0, buffer.Length);
                if (!WriteProcessMemory(Process.Handle, address, buffer, buffer.Length, IntPtr.Zero))
                    throw new Win32Exception();
            }
            catch
            {
                Free(address);
            }
            finally
            {
                Marshal.FreeHGlobal(hObj);
            }

            return address;
        }

        public void Write<T>(IntPtr address, T value) where T : struct
        {
            var buffer = new byte[Marshal.SizeOf(value)];
            var hObj   = Marshal.AllocHGlobal(buffer.Length);
            try
            {
                Marshal.StructureToPtr(value, hObj, false);
                Marshal.Copy(hObj, buffer, 0, buffer.Length);
                if (!WriteProcessMemory(Process.Handle, address, buffer, buffer.Length, IntPtr.Zero))
                    throw new Win32Exception();
            }
            finally
            {
                Marshal.FreeHGlobal(hObj);
            }
        }

        public IntPtr Write(byte[] buffer)
        {
            var addr = Alloc(buffer.Length);
            if (addr == IntPtr.Zero)
                throw new Win32Exception();
            Write(addr, buffer);
            return addr;
        }

        public void Write(IntPtr address, byte[] buffer)
        {
            if (!WriteProcessMemory(Process.Handle, address, buffer, buffer.Length, IntPtr.Zero))
                throw new Win32Exception();
        }

        public void WriteCString(IntPtr address, string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str + '\0');
            if (!WriteProcessMemory(Process.Handle, address, buffer, buffer.Length, IntPtr.Zero))
                throw new Win32Exception();
        }

        public IntPtr WriteCString(string str, Encoding encoding)
        {
            var buffer = encoding.GetBytes(str + '\0');
            var address = Alloc(buffer.Length);
            if (!WriteProcessMemory(Process.Handle, address, buffer, buffer.Length, IntPtr.Zero))
                throw new Win32Exception();
            return address;
        }

        public IntPtr WriteCString(string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str + '\0');
            var address = Alloc(buffer.Length);
            if (!WriteProcessMemory(Process.Handle, address, buffer, buffer.Length, IntPtr.Zero))
                throw new Win32Exception();
            return address;
        }
        /*
        public void Call(IntPtr address, params int[] funcArgs)
        {
            var tHandle = OpenThread(ThreadAccess.All, false, this.Process.Threads[0].Id);
            if (SuspendThread(tHandle) == 0xFFFFFFFF)
                throw new Win32Exception();

            var context = new CONTEXT { ContextFlags = ContextFlags.Control };
            if (!GetThreadContext(tHandle, ref context))
                throw new Win32Exception();

            var retaddr = Write<uint>(0xDEAD);

            var bytes = new List<byte>();

            #region ASM

            // push eip (stored refernse to next inctruction)
            bytes.Add(0x68);
            bytes.AddRange(BitConverter.GetBytes(context.Eip));

            // pushad (stored general registers)
            bytes.Add(0x60);
            // pushfd (stored flags)
            bytes.Add(0x9C);

            // pushed to the stack function arguments
            for (int i = funcArgs.Length - 1; i >= 0; --i)
            {
                if (funcArgs[i] == 0)
                {
                    // push 0
                    bytes.Add(0x6A);
                    bytes.Add(0x00);
                }
                else
                {
                    // push address
                    bytes.Add(0x68);
                    bytes.AddRange(BitConverter.GetBytes(funcArgs[i]));
                }
            }

            // mov eax, address
            var addr = this.Process.MainModule.BaseAddress.ToInt32() + address.ToInt32();
            bytes.Add(0xB8);
            bytes.AddRange(BitConverter.GetBytes(addr));

            // call eax
            bytes.Add(0xFF);
            bytes.Add(0xD0);

            // add esp, arg_count * pointersize (__cdecl correct stack)
            bytes.Add(0x83);
            bytes.Add(0xC4);
            bytes.Add((byte)(funcArgs.Length * IntPtr.Size));

            // mov [retaddr], eax
            bytes.Add(0xA3);
            bytes.AddRange(BitConverter.GetBytes(retaddr.ToInt32()));

            // popfd (restore flags)
            bytes.Add(0x9D);
            // popad (restore general registers)
            bytes.Add(0x61);
            // retn
            bytes.Add(0xC3);

            #endregion

            var injAddress = new IntPtr(this.Process.MainModule.BaseAddress.ToInt32() + Offsets.Default.InjectedAddress);
            var oldProtect = MemoryProtection.ReadOnly;

            // Save original code and disable protect
            var oldCode = this.ReadBytes(injAddress, bytes.Count);
            if (!VirtualProtectEx(this.Process.Handle, injAddress, bytes.Count, MemoryProtection.ExecuteReadWrite, out oldProtect))
                throw new Win32Exception();

            this.Write(injAddress, bytes.ToArray());

            context.Eip          = (uint)injAddress.ToInt32();
            context.ContextFlags = ContextFlags.Control;

            if (!SetThreadContext(tHandle, ref context) || ResumeThread(tHandle) == 0xFFFFFFFF)
                throw new Win32Exception();

            for (int i = 0; i < 0x100; ++i)
            {
                System.Threading.Thread.Sleep(15);
                if (this.Read<uint>(retaddr) != 0xDEAD)
                    break;
            }

            // restore protection and original code
            this.Write(injAddress, oldCode);
            if (!VirtualProtectEx(this.Process.Handle, injAddress, bytes.Count, oldProtect, out oldProtect))
                throw new Win32Exception();

            this.Free(retaddr);
        }
        */
        public IntPtr Rebase(IntPtr offset)
        {
            return new IntPtr(offset.ToInt64() + Process.MainModule.BaseAddress.ToInt64());
        }

        public bool IsFocusWindow
        {
            get { return Process.MainWindowHandle == GetForegroundWindow(); }
        }

    }

    #region Enums

    [Flags]
    public enum AllocationType : uint
    {
        Commit     = 0x00001000,
        Reserve    = 0x00002000,
        Decommit   = 0x00004000,
        Release    = 0x00008000,
        Reset      = 0x00080000,
        TopDown    = 0x00100000,
        WriteWatch = 0x00200000,
        Physical   = 0x00400000,
        LargePages = 0x20000000,
    }

    [Flags]
    public enum MemoryProtection : uint
    {
        NoAccess                 = 0x001,
        ReadOnly                 = 0x002,
        ReadWrite                = 0x004,
        WriteCopy                = 0x008,
        Execute                  = 0x010,
        ExecuteRead              = 0x020,
        ExecuteReadWrite         = 0x040,
        ExecuteWriteCopy         = 0x080,
        GuardModifierflag        = 0x100,
        NoCacheModifierflag      = 0x200,
        WriteCombineModifierflag = 0x400,
    }

    [Flags]
    public enum FreeType : uint
    {
        Decommit = 0x4000,
        Release  = 0x8000,
    }

    [Flags]
    public enum ThreadAccess : uint
    {
        Terminate           = 0x00001,
        SuspendResume       = 0x00002,
        GetContext          = 0x00008,
        SetContext          = 0x00010,
        SetInformation      = 0x00020,
        QueryInformation    = 0x00040,
        SetThreadToken      = 0x00080,
        Impersonate         = 0x00100,
        DirectImpersonation = 0x00200,
        All                 = 0x1F03FF
    }

    [Flags]
    public enum ContextFlags : uint
    {
        i386              = 0x10000,
        i486              = 0x10000,        // same as i386
        Control           = i386    | 0x01, // SS:SP, CS:IP, FLAGS, BP
        Integer           = i386    | 0x02, // AX, BX, CX, DX, SI, DI
        Segments          = i386    | 0x04, // DS, ES, FS, GS
        FloatingPoint     = i386    | 0x08, // 387 state
        DebugRegisters    = i386    | 0x10, // DB 0-3,6,7
        ExtendedRegisters = i386    | 0x20, // cpu specific extensions
        Full              = Control | Integer | Segments,
        All               = Control | Integer | Segments | FloatingPoint | DebugRegisters | ExtendedRegisters
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONTEXT
    {
        public ContextFlags ContextFlags; //set this to an appropriate value
        // Retrieved by CONTEXT_DEBUG_REGISTERS
        public uint Dr0;
        public uint Dr1;
        public uint Dr2;
        public uint Dr3;
        public uint Dr6;
        public uint Dr7;
        // Retrieved by CONTEXT_FLOATING_POINT
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=112)]//512
        public byte[] FloatSave;
        // Retrieved by CONTEXT_SEGMENTS
        public uint SegGs;
        public uint SegFs;
        public uint SegEs;
        public uint SegDs;
        // Retrieved by CONTEXT_INTEGER
        public uint Edi;
        public uint Esi;
        public uint Ebx;
        public uint Edx;
        public uint Ecx;
        public uint Eax;
        // Retrieved by CONTEXT_CONTROL
        public uint Ebp;
        public uint Eip;
        public uint SegCs;
        public uint EFlags;
        public uint Esp;
        public uint SegSs;
        // Retrieved by CONTEXT_EXTENDED_REGISTERS
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] ExtendedRegisters;
    }

    #endregion
}