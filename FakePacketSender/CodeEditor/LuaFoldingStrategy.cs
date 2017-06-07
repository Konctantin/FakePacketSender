using System.Collections.Generic;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace FakePacketSender.CodeEditor
{
    public class LuaFoldingStrategy
    {
        const RegexOptions PatternRegexOption = RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline;

        Regex
            startPattern   = new Regex(@"(?<start>\b(function|while|if|for)\b|{|--\[\[)", PatternRegexOption),
            endPattern     = new Regex(@"(?<end>\b(end)\b|}|]])", PatternRegexOption),
            commentPattern = new Regex(@"^\s*--[^\[]", PatternRegexOption);

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document and updates the folding manager with them.
        /// </summary>
        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            var foldings = CreateNewFoldings(document, out int firstErrorOffset);
            manager.UpdateFoldings(foldings, firstErrorOffset);
        }

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;

            var foldings = new List<NewFolding>();
            var stack = new Stack<int>();

            foreach (var line in document.Lines)
            {
                var text = document.GetText(line.Offset, line.Length);

                // комментарии пропускаем
                if (commentPattern.IsMatch(text))
                    continue;

                foreach (Match match in startPattern.Matches(text))
                {
                    var element = match.Groups["start"];
                    if (element.Success)
                    {
                        stack.Push(line.EndOffset);
                    }
                }

                foreach (Match match in endPattern.Matches(text))
                {
                    var element = match.Groups["end"];
                    if (element.Success)
                    {
                        if (stack.Count > 0)
                        {
                            var first = stack.Pop();
                            var folding = new NewFolding(first, line.EndOffset);
                            foldings.Add(folding);
                        }
                        else
                        {
                            firstErrorOffset = line.Offset;
                        }
                    }
                }
            }

            if (stack.Count > 0)
            {
                firstErrorOffset = stack.Pop();
            }

            foldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return foldings;
        }
    }
}
