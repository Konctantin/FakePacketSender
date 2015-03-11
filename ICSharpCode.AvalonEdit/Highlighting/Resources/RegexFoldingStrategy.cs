using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ICSharpCode.AvalonEdit.Highlighting
{
    /// <summary>
    /// Allows producing foldings from a document based on braces.
    /// </summary>
    public class RegexFoldingStrategy : AbstractFoldingStrategy
    {
        private const RegexOptions PatternRegexOption = RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline;

        private Regex startPattern;
        private Regex endPattern;

        public string StartPattern
        {
            get { return startPattern != null ? startPattern.ToString() : null; }
            set { if (!string.IsNullOrWhiteSpace(value)) this.startPattern = new Regex(value, PatternRegexOption); }
        }

        public string EndPattern
        {
            get { return endPattern != null ? endPattern.ToString() : null; }
            set { if (!string.IsNullOrWhiteSpace(value)) this.endPattern = new Regex(value, PatternRegexOption); }
        }

        /// <summary>
        /// Creates a new BraceFoldingStrategy.
        /// </summary>
        public RegexFoldingStrategy()
        {
            this.StartPattern = @"(?<start>\b(function|while|if|for)\b|{|--\[\[)";
            this.EndPattern = @"(?<end>\b(end)\b|}|]])";
        }

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;

            var foldings = new List<NewFolding>();
            var stack = new Stack<int>();

            for (int i = 0; i < document.LineCount; ++i)
            {
                var line = document.Lines[i];

                // комментарии пропускаем
                if (line.Text.TrimStart().StartsWith("--") && !line.Text.TrimStart().StartsWith("--[["))
                    continue;

                foreach (Match match in startPattern.Matches(line.Text))
                {
                    var element = match.Groups["start"];
                    if (element.Success)
                    {
                        stack.Push(line.EndOffset);
                        break;
                    }
                }

                foreach (Match match in endPattern.Matches(line.Text))
                {
                    var element = match.Groups["end"];
                    if (element.Success)
                    {
                        if (stack.Count > 0)
                        {
                            var first = stack.Pop();
                            foldings.Add(new NewFolding(first, line.EndOffset));
                            break;
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