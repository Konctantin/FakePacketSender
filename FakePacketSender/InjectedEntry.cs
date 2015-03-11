using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using NLua;

namespace FakePacketSender
{
    public static class InjectedEntry
    {
        [STAThread]
        public static int Run(string scriptName)
        {
            try
            {
                App.StartupPath = new System.IO.FileInfo(scriptName).DirectoryName;

                Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

                Application app = new Application();
                var window = new MainWindow();
                app.Run(window);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 0;
        }
    }
}
