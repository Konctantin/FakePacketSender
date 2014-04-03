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
                Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
                Application app = new Application();
                app.Run(new MainWindow());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 0;
        }
    }
}
