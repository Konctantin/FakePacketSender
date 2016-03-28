using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
        const string fileName = "scripts.xml";
        private ObservableCollection<Script> scriptList = new ObservableCollection<Script>();

        private Thread taskThread;
        private Lua lua = new Lua();

        private readonly string fullFileName;

        public MainWindow()
        {
            InitializeComponent();

            ConsoleWriter.Initialize(Path.Combine(App.StartupPath, "log.log"), teLog, true);
            fullFileName = Path.Combine(App.StartupPath, fileName);

            try
            {
                Console.WriteLine("StartUp: " + App.StartupPath);

                RegisterFunctions();

                IntelliSienceManager.IntelliSienceCollection = new List<WowApi>() {
                    new WowApi() { Name = "CreateFakePacket", Signature = "packet = CreateFakePaket(sendOffset, opcode)",  Description = "Создает новый пакет для отправки серверу.", ImageType = ImageType.Method },
                    new WowApi() { Name = "WriteBits",        Signature = ":WriteBits(value, bitcount)",  Description = "Записывает в пакет значение типа uint с указанным количеством бит.", ImageType = ImageType.Method },
                    new WowApi() { Name = "WriteInt16",       Signature = ":WriteInt16(value)",  Description = "Записывает в пакет значение типа int16.", ImageType = ImageType.Method },
                    new WowApi() { Name = "WriteInt32",       Signature = ":WriteInt32(value)",  Description = "Записывает в пакет значение типа int32.", ImageType = ImageType.Method },
                    new WowApi() { Name = "WriteInt64",       Signature = ":WriteInt64(value)",  Description = "Записывает в пакет значение типа int64.", ImageType = ImageType.Method },
                    new WowApi() { Name = "WriteFloat",       Signature = ":WriteFloat(value)",  Description = "Записывает в пакет значение типа float.", ImageType = ImageType.Method },
                    new WowApi() { Name = "WriteBytes",       Signature = ":WriteBytes(...)",  Description = "Записывает в пакет последовательность байт.", ImageType = ImageType.Method },
                    new WowApi() { Name = "FillBytes",        Signature = ":FillBytes(value, count)",  Description = "Записывает массив указанного размера заполненого указанным значением.", ImageType = ImageType.Method },
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

                if (File.Exists(fullFileName))
                {
                    using (var file = File.Open(fullFileName, FileMode.Open))
                    {
                        scriptList = (ObservableCollection<Script>)new XmlSerializer(typeof(ObservableCollection<Script>)).Deserialize(file);
                        Console.WriteLine("ScriptCount: " + scriptList.Count);
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

            DataContext = scriptList;
        }

        private void RegisterFunctions()
        {
            lua.LoadCLRPackage();

            lua.RegisterFunction("sleep", typeof(Thread)
                .GetMethod("Sleep", new Type[] { typeof(int) }));

            lua.RegisterFunction("CreateFakePacket", typeof(FakePacket.FakePacket)
                // CreateFakePacket(sendOffset, opcode);
                .GetMethod("CreateFakePacket", new Type[] { typeof(int), typeof(int) }));

            var type = typeof(FakePacket.Extensions);
            lua.RegisterFunction("bor",  type.GetMethod("Bit_Or"));
            lua.RegisterFunction("bxor", type.GetMethod("Bit_Xor"));
            lua.RegisterFunction("band", type.GetMethod("Bit_And"));
            lua.RegisterFunction("bnot", type.GetMethod("Bit_Not"));
            lua.RegisterFunction("blsh", type.GetMethod("Bit_Lsh"));
            lua.RegisterFunction("brsh", type.GetMethod("Bit_Rsh"));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Save_Click(null, null);
            lua?.Dispose();
            Application.Current.Shutdown(0);
        }

        private void CommandBinding_New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var content = new StringBuilder();
            content.AppendLine("local sendOffset = 0x0; -- address of function ClientConnection::Send(CDataStore*)");
            content.AppendLine("local opcode = 0; -- Client message id");
            content.AppendLine("local packet = CreateFakePacket(sendOffset, opcode);");
            content.AppendLine("for i = 1, 1000 do");
            content.AppendLine("    packet:Clear();");
            content.AppendLine("    -- write packet data;");
            content.AppendLine("    print(\"Packet:\", packet:Dump());");
            content.AppendLine("    packet:Send();");
            content.AppendLine("    sleep(50);");
            content.AppendLine("end");

            scriptList.Add(new Script {
                Name = "<new>",
                Lua  = content.ToString()
            });
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
            using (var file = File.Open(fullFileName, FileMode.Create))
                new XmlSerializer(typeof(ObservableCollection<Script>)).Serialize(file, scriptList);
            Console.WriteLine("Сохранено!");
        }

        private void CommandBinding_Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            teLog.Clear();
            var code = $"function lua_main(build)\n{teCode.Text}\n end \n lua_main({CurentBuild})";
            CancellationToken token = new CancellationToken();

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
            taskThread?.Abort();
            taskThread = null;
        }

        private void CommandBinding_Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (taskThread == null)
                e.CanExecute = true;
            else if ((taskThread.ThreadState & (ThreadState.Running | ThreadState.Background)) == 0)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void CommandBinding_Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (taskThread == null)
                e.CanExecute = false;
            else if ((taskThread.ThreadState & (ThreadState.Running | ThreadState.Background)) != 0)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        public int CurentBuild => System.Diagnostics.Process.GetCurrentProcess()
            .MainModule.FileVersionInfo.FilePrivatePart;
    }
}
