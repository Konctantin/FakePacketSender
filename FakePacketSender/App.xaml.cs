using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace FakePacketSender
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var process = Process.GetProcessesByName("wow");
            var inj = new Inject.Injector(process[0], "ManagedHost.dll");
            this.Shutdown(1);
        }
    }
}
