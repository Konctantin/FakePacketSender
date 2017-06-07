using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;

namespace FakePacketSender.CodeEditor.Bracket
{
    public class BracketHighlightRenderer : IBackgroundRenderer
    {
        BracketSearchResult result;
        Pen borderPen;
        Brush backgroundBrush;
        TextView textView;

        public KnownLayer Layer => KnownLayer.Selection;

        public void SetHighlight(BracketSearchResult result)
        {
            if (this.result != result)
            {
                this.result = result;
                textView.InvalidateLayer(Layer);
            }
        }

        public BracketHighlightRenderer(TextView textView)
        {
            if (textView == null)
                throw new ArgumentNullException(nameof(textView));

            this.textView = textView;
            this.textView.BackgroundRenderers.Add(this);

            borderPen = new Pen(new SolidColorBrush(Color.FromArgb(0xFF, 0x71, 0x0B, 0xCB)), 1);
            backgroundBrush = new SolidColorBrush(Color.FromArgb(0xFF,0x71,0x0B,0xCB));
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (result == null)
                return;

            var builder = new BackgroundGeometryBuilder();
            builder.AlignToWholePixels = true;
            builder.AddSegment(textView, new TextSegment { StartOffset = result.OpeningOffset, Length = 1 });
            builder.CloseFigure(); // prevent connecting the two segments
            builder.AddSegment(textView, new TextSegment { StartOffset = result.ClosingOffset, Length = 1 });

            var geometry = builder.CreateGeometry();
            if (geometry != null)
                drawingContext.DrawGeometry(backgroundBrush, borderPen, geometry);
        }

        public static BracketHighlightRenderer Install(TextArea textArea)
        {
            var bracketSearcher = new BracketSearcher();
            var bracketRenderer = new BracketHighlightRenderer(textArea.TextView);

            EventHandler handler = (o, e) => {
                var result = bracketSearcher.SearchBracket(textArea.Document, textArea.Caret.Offset);
                bracketRenderer.SetHighlight(result);
            };

            textArea.Caret.PositionChanged += handler;
            return bracketRenderer;
        }
    }
}
