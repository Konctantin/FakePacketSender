
namespace FakePacketSender.CodeEditor.Bracket
{
    public class BracketSearchResult
    {
        public int OpeningOffset { get; }

        public int ClosingOffset { get; }

        public int DefinitionHeaderOffset { get; set; }

        public int DefinitionHeaderLength { get; set; }

        public BracketSearchResult(int openingOffset, int closingOffset)
        {
            OpeningOffset = openingOffset;
            ClosingOffset = closingOffset;
        }
    }
}