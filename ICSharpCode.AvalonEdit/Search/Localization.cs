namespace ICSharpCode.AvalonEdit.Search
{
    /// <summary>
    /// Holds default texts for buttons and labels in the SearchPanel. Override properties to add other languages.
    /// </summary>
    public class Localization
    {
        /// <summary>
        /// Default: 'Match case'
        /// </summary>
        public virtual string MatchCaseText
        {
            get { return "Учитывать регистр"; }
        }

        /// <summary>
        /// Default: 'Match whole words'
        /// </summary>
        public virtual string MatchWholeWordsText
        {
            get { return "Искать слова целиком"; }
        }

        /// <summary>
        /// Default: 'Use regular expressions'
        /// </summary>
        public virtual string UseRegexText
        {
            get { return "Использовать регилярные выражения"; }
        }

        /// <summary>
        /// Default: 'Find next (F3)'
        /// </summary>
        public virtual string FindNextText
        {
            get { return "Найти далее (F3)"; }
        }

        /// <summary>
        /// Default: 'Find previous (Shift+F3)'
        /// </summary>
        public virtual string FindPreviousText
        {
            get { return "Найти предыдущее (Shift+F3)"; }
        }

        /// <summary>
        /// Default: 'Error: '
        /// </summary>
        public virtual string ErrorText
        {
            get { return "Ошибка: "; }
        }

        /// <summary>
        /// Default: 'No matches found!'
        /// </summary>
        public virtual string NoMatchesFoundText
        {
            get { return "Совпадений не найдено!"; }
        }
    }
}