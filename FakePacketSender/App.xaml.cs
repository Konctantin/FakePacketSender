using System;
using System.Diagnostics;
using System.Windows;
using FakePacketSender.Inject;
using System.Reflection;
using System.IO;

namespace FakePacketSender
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string StartupPath { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length == 1 && e.Args[0] == "/ui")
            {
                StartupPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Current.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
                return;
            }

            var process = Process.GetProcessesByName("wow");
            try
            {
                int processIndex = 0;

                if (process.Length == 0)
                    throw new Exception("Process \"wow\" not found!");

                if (process.Length > 1)
                {
                    var dialog = new SelectProcessDialog();
                    dialog.cbProcess.Items.Clear();
                    foreach (var proc in process)
                    {
                        var item = string.Format("({0}) {1}", proc.Id, proc.ProcessName);
                        dialog.cbProcess.Items.Add(item);
                    }
                    dialog.cbProcess.SelectedIndex = 0;
                    if (dialog.ShowDialog() == true)
                    {
                        processIndex = dialog.cbProcess.SelectedIndex;
                    }
                    else
                    {
                        processIndex = -1;
                    }
                }

                if (processIndex >= 0)
                {
                    new Injector(
                        process[processIndex],
                        "ManagedHost.dll");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop);
            }
            finally
            {
                Shutdown(1);
            }
        }
    }
}
