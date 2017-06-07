using System;
using System.IO;
using System.Windows.Threading;
using System.Xml;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using FakePacketSender.CodeEditor.Bracket;

namespace FakePacketSender.CodeEditor
{
    public class LuaCodeEditor : TextEditor, IDisposable
    {
        FoldingManager foldingManager;
        DispatcherTimer foldingUpdateTimer;
        LuaFoldingStrategy foldingStrategy = new LuaFoldingStrategy();

        public LuaCodeEditor()
            : base(new TextArea() { Document = new ICSharpCode.AvalonEdit.Document.TextDocument() })
        {
            Options.ShowTabs = true;
            Options.ConvertTabsToSpaces = true;

            InstallFoldingManager();
            InstallHighlighting();

            SearchPanel.Install(TextArea);
            BracketHighlightRenderer.Install(TextArea);

            TextArea.IndentationStrategy = new LuaIndentationStrategy(this);
        }

        public void Dispose()
        {
            foldingUpdateTimer?.Stop();
        }

        void InstallHighlighting()
        {
            using (XmlReader reader = new XmlTextReader(new StringReader(Properties.Resources.LUA)))
            {
                var luaHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                HighlightingManager.Instance.RegisterHighlighting("Lua", new[] { ".lua" }, luaHighlighting);
                SyntaxHighlighting = luaHighlighting;
            }
        }

        void InstallFoldingManager()
        {
            foldingManager = FoldingManager.Install(TextArea);

            foldingUpdateTimer = new DispatcherTimer();
            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(1);
            foldingUpdateTimer.Tick += (o, e) =>
            {
                if (Document != null)
                    foldingStrategy.UpdateFoldings(foldingManager, Document);
            };
            foldingUpdateTimer.Start();

            TextArea.DocumentChanged += (o, e) => {
                if (foldingManager != null)
                    FoldingManager.Uninstall(foldingManager);
                foldingManager = FoldingManager.Install(o as TextArea);
            };
        }

        #region Temporary hack
#warning hack (todo: implement syntax tree)

        public string GetWord(int offset = -1)
        {
            Func<char, bool> isKeyWord = (c) => (c >= 'A' && c <= 'z') || (c >= '0' && c <= '9') || c == '_' || c == ':' || c == '.';

            if (offset == -1)
                offset = SelectionStart;

            if (offset < 0 || offset >= Text.Length)
                return null;

            int start = 0, len = 0;
            for (start = offset - 1; start >= 0; start--)
            {
                if (!isKeyWord(Text[start]))
                {
                    ++start;
                    break;
                }
            }
            start = Math.Max(start, 0);
            for (int j = start; j < Text.Length; ++j, ++len)
            {
                if (!isKeyWord(Text[j]))
                    break;
            }
            var word = Text.Substring(start, len);
            return string.IsNullOrWhiteSpace(word) ? null : word.Trim();
        }

        #endregion
    }
}
