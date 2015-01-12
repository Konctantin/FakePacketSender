using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using FakePacketSender.Properties;
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

        private Thread taskThread;
        private Lua lua = new Lua();
        public MainWindow()
        {
            InitializeComponent();

            ConsoleWriter.Initialize(Path.Combine(App.StartupPath, "log.log"), teLog, true);

            try
            {
                tbSend2.Text = "0x" + Settings.Default.Send2.ToString("X");
                FakePacket.FakePacket.Send2Addr = Settings.Default.Send2;

                Console.WriteLine("Send2 address: 0x{0:X}", FakePacket.FakePacket.Send2Addr);

                RegisterFunctions();

                IntelliSienceManager.IntelliSienceCollection = new List<WowApi>() {
                    new WowApi() { Name = "CreateFakePacket", Signature = "packet = CreateFakePaket(opcode)",  Description = "Создает новый пакет для отправки серверу.", ImageType = ImageType.Method },
                    new WowApi() { Name = "WriteBits",        Signature = ":WriteBits(value, bitcount)",  Description = "Записывает в пакет значение типа uint с указанным количеством бит.", ImageType = ImageType.Method },
                    new WowApi() { Name = "WriteInt32",       Signature = ":WriteInt32(value)",  Description = "Записывает в пакет значение типа int.", ImageType = ImageType.Method },
                    new WowApi() { Name = "WriteFloat",       Signature = ":WriteFloat(value)",  Description = "Записывает в пакет значение типа float.", ImageType = ImageType.Method },
                    new WowApi() { Name = "WriteBytes",       Signature = ":WriteBytes(...)",  Description = "Записывает в пакет последовательность байт.", ImageType = ImageType.Method },
                    new WowApi() { Name = "Clear",            Signature = ":Clear()",  Description = "Очищает пакет от данных.", ImageType = ImageType.Method },
                    new WowApi() { Name = "Send",             Signature = ":Send()",  Description = "Отправляет данный пакет серверу.", ImageType = ImageType.Method },
                    new WowApi() { Name = "Dump",             Signature = "dump = packet:Dump()",  Description = "Возвращает дамп пакета в виде %02Х.", ImageType = ImageType.Method },
                    new WowApi() { Name = "sleep",            Signature = "sleep(ms)",  Description = "Приостанавливает работу потока на указанное количество милисекунд.", ImageType = ImageType.Method },

                    new WowApi() { Name = "bor",             Signature = "val = bor(lval, rval)",   Description = "val = lval | rval",  ImageType = ImageType.Method },
                    new WowApi() { Name = "bxor",            Signature = "val = bxor(lval, rval)",  Description = "val = lval ^ rval",  ImageType = ImageType.Method },
                    new WowApi() { Name = "band",            Signature = "val = band(lval, rval)",  Description = "val = lval & rval",  ImageType = ImageType.Method },
                    new WowApi() { Name = "bnot",            Signature = "val = bnot(lval, rval)",  Description = "val = lval & ~rval", ImageType = ImageType.Method },
                    new WowApi() { Name = "blsh",            Signature = "val = blsh(lval, rval)",  Description = "val = lval << rval", ImageType = ImageType.Method },
                    new WowApi() { Name = "brsh",            Signature = "val = brsh(lval, rval)",  Description = "val = lval >> rval", ImageType = ImageType.Method },
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

            lua.RegisterFunction("bor",  typeof(FakePacket.Extensions).GetMethod("Bit_Or"));
            lua.RegisterFunction("bxor", typeof(FakePacket.Extensions).GetMethod("Bit_Xor"));
            lua.RegisterFunction("band", typeof(FakePacket.Extensions).GetMethod("Bit_And"));
            lua.RegisterFunction("bnot", typeof(FakePacket.Extensions).GetMethod("Bit_Not"));
            lua.RegisterFunction("blsh", typeof(FakePacket.Extensions).GetMethod("Bit_Lsh"));
            lua.RegisterFunction("brsh", typeof(FakePacket.Extensions).GetMethod("Bit_Rsh"));
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

        private void CommandBinding_New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            scriptList.Add(new Script { Name = "<new>", Lua = "-- local packet = CreateFakePacket(0);" });
        }

        private void CommandBinding_Delete_Executed(object sender, ExecutedRoutedEventArgs e)
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

            int offset = 0;
            if (int.TryParse(tbSend2.Text.Substring(2), NumberStyles.AllowHexSpecifier, null, out offset))
                Settings.Default.Send2 = offset;
            FakePacketSender.Properties.Settings.Default.Save();
            Console.WriteLine("Сохранено!");
        }

        private void CommandBinding_Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            teLog.Clear();
            var code = teCode.Text;
            Task.Run(() =>
            {
                taskThread = Thread.CurrentThread;
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

        private void CommandBinding_Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (taskThread != null)
                taskThread.Abort();
            taskThread = null;
        }

        private void CommandBinding_Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (taskThread == null) || (taskThread.ThreadState != System.Threading.ThreadState.Running);
        }

        private void CommandBinding_Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (taskThread != null) && (taskThread.ThreadState == System.Threading.ThreadState.Running);
        }
    }
}
