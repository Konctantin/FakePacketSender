using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NLua;

namespace FakePacketSender
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Lua lua = new Lua();
        public MainWindow()
        {
            InitializeComponent();
            ConsoleWriter.Initialize("log.log", teLog, true);
            try
            {
                RegisterFunctions();
            }
            catch (Exception ex)
            {
            }
        }

        private void RegisterFunctions()
        {
            //Thread.Sleep()
            lua.LoadCLRPackage();

            lua.RegisterFunction("sleep", typeof(Thread)
                .GetMethod("Sleep", new Type[] { typeof(int) }));

            lua.RegisterFunction("CreateFakePacket", typeof(FakePacket.FakePacket)
                .GetMethod("CreateFakePacket", new Type[] { typeof(int) }));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (lua != null)
            {
                lua.Dispose();
            }
            Application.Current.Shutdown(0);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var code = teCode.Text; 
            await Task.Run(() => {
                try
                {
                    lua.DoString(code);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
