using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using ICSharpCode.AvalonEdit.CodeCompletion;
using NLua;

namespace FakePacketSender
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Script> scriptList = new ObservableCollection<Script>();

        private Lua lua = new Lua();
        public MainWindow()
        {
            InitializeComponent();

            ConsoleWriter.Initialize(Path.Combine(App.StartupPath, "log.log"), teLog, true);

            try
            {
                App.Offsets = new Offsets() { Send2 = 0x1090, VTable = 0 };

                RegisterFunctions();

                IntelliSienceManager.IntelliSienceCollection = new List<WowApi>()
                {
                    new WowApi() { Name = "CreateFakePacket", Signature = "packet = CreateFakePaket(opcode)",  Description = "Создает новый пакет для отправки серверу.", ImageType = ImageType.Method },
                    new WowApi() { Name = "WriteBits",        Signature = ":WriteBits(value, bitcount)",  Description = "Записывает в пакет значение типа uint с указанным количеством бит.", ImageType = ImageType.Method },
                    new WowApi() { Name = "WriteInt32",       Signature = ":WriteInt32(value)",  Description = "Записывает в пакет значение типа int.", ImageType = ImageType.Method },
                    new WowApi() { Name = "WriteFloat",       Signature = ":WriteFloat(value)",  Description = "Записывает в пакет значение типа float.", ImageType = ImageType.Method },
                    new WowApi() { Name = "WriteBytes",       Signature = ":WriteBytes(...)",  Description = "Записывает в пакет последовательность байт.", ImageType = ImageType.Method },
                    new WowApi() { Name = "Clear",            Signature = ":Clear()",  Description = "Очищает пакет от данных.", ImageType = ImageType.Method },
                    new WowApi() { Name = "Send",             Signature = ":Send()",   Description = "Отправляет данный пакет серверу.", ImageType = ImageType.Method },
                    new WowApi() { Name = "sleep",            Signature = "sleep(ms)", Description="Приостанавливает поток на указанное количество милисекунд.", ImageType = ImageType.Method },
                };

                if (File.Exists(Path.Combine(App.StartupPath, "sctipts.xml")))
                {
                    using (var file = File.Open(Path.Combine(App.StartupPath, "sctipts.xml"), FileMode.Open))
                    {
                        scriptList = (ObservableCollection<Script>)new XmlSerializer(typeof(ObservableCollection<Script>)).Deserialize(file);
                    }
                }
                else
                {
                    scriptList.Add(new Script { Name = "<new>", Lua = "-- local packet = CreateFakePacket(0);" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            this.DataContext = scriptList;
        }

        private void RegisterFunctions()
        {
            lua.LoadCLRPackage();

            lua.RegisterFunction("sleep", typeof(Thread)
                .GetMethod("Sleep", new Type[] { typeof(int) }));

            lua.RegisterFunction("CreateFakePacket", typeof(FakePacket.FakePacket)
                .GetMethod("CreateFakePacket", new Type[] { typeof(int) }));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Save_Click(null, null);

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
            // todo
        }

        private void CommandBinding_New_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            scriptList.Add(new Script { Name = "<new>", Lua = "-- local packet = CreateFakePacket(0);" });
        }

        private void CommandBinding_Delete_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (lbScripts.SelectedIndex > -1)
            {
                scriptList.RemoveAt(lbScripts.SelectedIndex);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            using (var file = File.Open(Path.Combine(App.StartupPath, "sctipts.xml"), FileMode.Create))
                new XmlSerializer(typeof(ObservableCollection<Script>)).Serialize(file, scriptList);
            Console.WriteLine("Сохранено!");
        }
    }
}
