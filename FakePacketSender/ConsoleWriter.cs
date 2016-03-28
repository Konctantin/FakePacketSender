using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;


namespace FakePacketSender
{
    public class ConsoleWriter : TextWriter
    {
        private static ConsoleWriter Instance;
        private StreamWriter m_writer;
        private TextBox Editor;

        public ConsoleWriter(string fileName, TextBox editor, bool isRegisterUnhandledException = false)
        {
            m_writer = new StreamWriter(fileName, false, Encoding);
            Editor = editor;
            m_writer.AutoFlush = true;
            Console.SetOut(this);
            Debug.Listeners.Add(new TextWriterTraceListener(this));

            if (isRegisterUnhandledException)
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        public static void Initialize(string fileName, bool isRegisterUnhandledException = false)
        {
            if (Instance != null)
                throw new InvalidOperationException("ConsoleWriter is already initialized.");

            Instance = new ConsoleWriter(fileName, null, isRegisterUnhandledException);
        }

        public static void Initialize(string fileName, TextBox editor, bool isRegisterUnhandledException = false)
        {
            if (Instance != null)
                throw new InvalidOperationException("ConsoleWriter is already initialized.");

            Instance = new ConsoleWriter(fileName, editor, isRegisterUnhandledException);
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override void WriteLine(string value)
        {
            InternalWrite(value + Environment.NewLine);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            InternalWrite(string.Format(format, arg) + Environment.NewLine);
        }

        public override void WriteLine()
        {
            InternalWrite(Environment.NewLine);
        }

        public override void Write(string value)
        {
            InternalWrite(value);
        }

        public override void Write(string format, params object[] arg)
        {
            InternalWrite(string.Format(format, arg));
        }

        public override void Close()
        {
            base.Close();
            if (m_writer != null)
            {
                Debug.Listeners.Clear();
                m_writer.Close();
                m_writer = null;
            }
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        }

        private void InternalWrite(string text)
        {
            var content = string.Format("[{0:HH:mm:ss.fff}] {1}", DateTime.Now, text);

            if (m_writer != null)
                m_writer.Write(content);

            if (Editor != null)
            {
                Editor.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(()=> {
                    Editor.AppendText(text);
                    Editor.ScrollToEnd();
                }));
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                Console.WriteLine(exception);
            }
        }

        public static void CloseWriter()
        {
            if (Instance != null)
                Instance.Close();
        }
    }
}