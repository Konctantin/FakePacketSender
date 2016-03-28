using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FakePacketSender.Inject
{
    internal class Injector : ProcessMemory
    {
        #region API

        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, int lpThreadAttributes, int dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
        [DllImport("kernel32", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
        [DllImport("kernel32")]
        static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);
        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibraryA(string lpFileName);
        [DllImport("kernel32", SetLastError = true)]
        static extern int FreeLibrary(IntPtr handle);

        #endregion

        private string HostDllName;

        protected IntPtr CreateRemoteThread(IntPtr func, IntPtr arg)
        {
            return CreateRemoteThread(Process.Handle, 0, 0, func, arg, 0, IntPtr.Zero);
        }

        private IntPtr GetHostFuncAddr(string func, ref IntPtr moduleHandle)
        {
            var hLoaded  = LoadLibraryA(HostDllName);
            var lpInject = GetProcAddress(hLoaded, func);
            var offset   = lpInject.ToInt32() - hLoaded.ToInt32();
            FreeLibrary(hLoaded);

            var hostAddr = Process.Modules.Cast<ProcessModule>()
                .Where(m => m.FileName == HostDllName)
                .FirstOrDefault();

            if (hostAddr == null)
                return IntPtr.Zero;

            moduleHandle = hostAddr.BaseAddress;

            return IntPtr.Add(hostAddr.BaseAddress, offset);
        }

        public Injector(Process process, string hostName)
            :base(process)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentNullException("hostName");

            HostDllName = Path.Combine(Environment.CurrentDirectory, hostName);
	        var fnLoadLibrary = GetProcAddress(GetModuleHandle("kernel32"), "LoadLibraryA");

            // load host
            var argAddr = WriteCString(HostDllName, Encoding.ASCII);
            var thread  = CreateRemoteThread(fnLoadLibrary, argAddr);

            WaitForSingleObject(thread, 0xFFFFFFFF);
            Free(argAddr);
            CloseHandle(thread);

            // call host func
            var injFileName = WriteCString(Path.Combine(Environment.CurrentDirectory, @"FakePacketSender.exe"));
            var moduleHandle = IntPtr.Zero;

            var injectFunc = GetHostFuncAddr("Inject", ref moduleHandle);
            if (injectFunc == IntPtr.Zero)
                throw new Exception("hostFunc == IntPtr.Zero");

            var thread2 = CreateRemoteThread(injectFunc, injFileName);

            WaitForSingleObject(thread2, 0xFFFFFFFF);
            Free(injFileName);
            CloseHandle(thread2);

            // free
            var fnFreeLibrary = GetProcAddress(GetModuleHandle("kernel32"), "FreeLibrary");
            var threadFree    = CreateRemoteThread(fnFreeLibrary, moduleHandle);

            WaitForSingleObject(threadFree, 0xFFFFFFFF);

            //var modules = string.Join("\r\n", this.Process.Modules.Cast<ProcessModule>().Select(m => m.ModuleName));
            //MessageBox.Show(modules);
        }
    }
}
