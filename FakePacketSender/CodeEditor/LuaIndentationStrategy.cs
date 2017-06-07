using ICSharpCode.AvalonEdit.Document;
using System;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit;

namespace FakePacketSender.CodeEditor
{
    public class LuaIndentationStrategy : DefaultIndentationStrategy
    {
        const int indent_space_count = 4;
        const string patternFull  = @"\b(?<start>function|while(.+)do|if(.+)then|elseif(.+)then|for(.+)do|{{|end|}})\b";
        const string patternStart = @"\b(?<start>function|while(.+)do|if(.+)then|elseif(.+)then|for(.+)do|{{)\b";
        const string patternEnd   = @"\b(?<start>end)\b";

        TextEditor textEditor;

        public LuaIndentationStrategy(TextEditor textEditor)
        {
            this.textEditor = textEditor;
        }

        int CalcSpace(string str)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                if (!char.IsWhiteSpace(str[i]))
                    return i;
                if (i == str.Length - 1)
                    return str.Length;
            }
            return 0;
        }

        /// <inheritdoc cref="IIndentationStrategy.IndentLine"/>
        public override void IndentLine(TextDocument document, DocumentLine line)
        {
            if (line?.PreviousLine == null)
                return;

            var prevLine = document.GetText(line.PreviousLine.Offset, line.PreviousLine.Length);
            var curLine = document.GetText(line.Offset, line.Length);
            int prev = CalcSpace(prevLine);

            var previousIsComment = prevLine.TrimStart().StartsWith("--", StringComparison.CurrentCulture);

            if (Regex.IsMatch(curLine, patternFull) && !previousIsComment)
            {
                var ind = new string(' ', prev);
                document.Insert(line.Offset, ind);
            }
            else if (Regex.IsMatch(prevLine, patternStart) && !previousIsComment)
            {
                var ind = new string(' ', prev + indent_space_count);
                document.Insert(line.Offset, ind);

                var found = false;
                for (int i = line.LineNumber; i < document.LineCount; ++i)
                {
                    var text = document.GetText(document.Lines[i].Offset, document.Lines[i].Length);

                    if (string.IsNullOrWhiteSpace(text) || text.TrimStart().StartsWith("--", StringComparison.CurrentCulture))
                        continue;

                    var sps = CalcSpace(text);

                    if (sps == prev && Regex.IsMatch(text, patternEnd))
                        found = true;
                }

                if (!found)
                {
                    var ntext = Environment.NewLine + new string(' ', prev) + "end";
                    var point = textEditor.SelectionStart;
                    document.Insert(line.Offset + ind.Length, ntext);
                    textEditor.SelectionStart = point;
                }
            }
            else
            {
                var ind = new string(' ', prev);
                if (line != null)
                    document.Insert(line.Offset, ind);
            }
        }
    }
}