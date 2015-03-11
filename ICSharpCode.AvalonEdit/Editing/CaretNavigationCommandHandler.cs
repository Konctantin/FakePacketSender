using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;

namespace ICSharpCode.AvalonEdit.Editing
{
    internal static class CaretNavigationCommandHandler
    {
        /// <summary>
        /// Creates a new <see cref="TextAreaInputHandler"/> for the text area.
        /// </summary>
        public static TextAreaInputHandler Create(TextArea textArea)
        {
            TextAreaInputHandler handler = new TextAreaInputHandler(textArea);
            handler.CommandBindings.AddRange(CommandBindings);
            handler.InputBindings.AddRange(InputBindings);
            return handler;
        }

        private static readonly List<CommandBinding> CommandBindings = new List<CommandBinding>();
        private static readonly List<InputBinding>   InputBindings   = new List<InputBinding>();

        private static void AddBinding(ICommand command, ModifierKeys modifiers, Key key, ExecutedRoutedEventHandler handler)
        {
            CommandBindings.Add(new CommandBinding(command, handler));
            InputBindings.Add(TextAreaDefaultInputHandler.CreateFrozenKeyBinding(command, modifiers, key));
        }

        static CaretNavigationCommandHandler()
        {
            const ModifierKeys None  = ModifierKeys.None;
            const ModifierKeys Ctrl  = ModifierKeys.Control;
            const ModifierKeys Shift = ModifierKeys.Shift;
            const ModifierKeys Alt   = ModifierKeys.Alt;

            AddBinding(EditingCommands.MoveLeftByCharacter,     None,           Key.Left,       OnMoveCaret(CaretMovementType.CharLeft));
            AddBinding(EditingCommands.SelectLeftByCharacter,   Shift,          Key.Left,       OnMoveCaretExtendSelection(CaretMovementType.CharLeft));
            AddBinding(EditingCommands.MoveRightByCharacter,    None,           Key.Right,      OnMoveCaret(CaretMovementType.CharRight));
            AddBinding(EditingCommands.SelectRightByCharacter,  Shift,          Key.Right,      OnMoveCaretExtendSelection(CaretMovementType.CharRight));

            AddBinding(EditingCommands.MoveLeftByWord,          Ctrl,           Key.Left,       OnMoveCaret(CaretMovementType.WordLeft));
            AddBinding(EditingCommands.SelectLeftByWord,        Ctrl | Shift,   Key.Left,       OnMoveCaretExtendSelection(CaretMovementType.WordLeft));
            AddBinding(EditingCommands.MoveRightByWord,         Ctrl,           Key.Right,      OnMoveCaret(CaretMovementType.WordRight));
            AddBinding(EditingCommands.SelectRightByWord,       Ctrl | Shift,   Key.Right,      OnMoveCaretExtendSelection(CaretMovementType.WordRight));

            AddBinding(EditingCommands.MoveUpByLine,            None,           Key.Up,         OnMoveCaret(CaretMovementType.LineUp));
            AddBinding(EditingCommands.SelectUpByLine,          Shift,          Key.Up,         OnMoveCaretExtendSelection(CaretMovementType.LineUp));
            AddBinding(EditingCommands.MoveDownByLine,          None,           Key.Down,       OnMoveCaret(CaretMovementType.LineDown));
            AddBinding(EditingCommands.SelectDownByLine,        Shift,          Key.Down,       OnMoveCaretExtendSelection(CaretMovementType.LineDown));

            AddBinding(EditingCommands.MoveDownByPage,          None,           Key.PageDown,   OnMoveCaret(CaretMovementType.PageDown));
            AddBinding(EditingCommands.SelectDownByPage,        Shift,          Key.PageDown,   OnMoveCaretExtendSelection(CaretMovementType.PageDown));
            AddBinding(EditingCommands.MoveUpByPage,            None,           Key.PageUp,     OnMoveCaret(CaretMovementType.PageUp));
            AddBinding(EditingCommands.SelectUpByPage,          Shift,          Key.PageUp,     OnMoveCaretExtendSelection(CaretMovementType.PageUp));

            AddBinding(EditingCommands.MoveToLineStart,         None,           Key.Home,       OnMoveCaret(CaretMovementType.LineStart));
            AddBinding(EditingCommands.SelectToLineStart,       Shift,          Key.Home,       OnMoveCaretExtendSelection(CaretMovementType.LineStart));
            AddBinding(EditingCommands.MoveToLineEnd,           None,           Key.End,        OnMoveCaret(CaretMovementType.LineEnd));
            AddBinding(EditingCommands.SelectToLineEnd,         Shift,          Key.End,        OnMoveCaretExtendSelection(CaretMovementType.LineEnd));

            AddBinding(RectangleSelectionCommand.MoveUp,        Alt | Shift,    Key.Up,         OnMoveCaretExtendSelection(CaretMovementType.MultyUp));
            AddBinding(RectangleSelectionCommand.MoveDown,      Alt | Shift,    Key.Down,       OnMoveCaretExtendSelection(CaretMovementType.MultyDown));
            AddBinding(RectangleSelectionCommand.MoveLeft,      Alt | Shift,    Key.Left,       OnMoveCaretExtendSelection(CaretMovementType.MultyLeft));
            AddBinding(RectangleSelectionCommand.MoveRight,     Alt | Shift,    Key.Right,      OnMoveCaretExtendSelection(CaretMovementType.MultyRight));

            AddBinding(EditingCommands.MoveToDocumentStart,     Ctrl,           Key.Home,       OnMoveCaret(CaretMovementType.DocumentStart));
            AddBinding(EditingCommands.SelectToDocumentStart,   Ctrl | Shift,   Key.Home,       OnMoveCaretExtendSelection(CaretMovementType.DocumentStart));
            AddBinding(EditingCommands.MoveToDocumentEnd,       Ctrl,           Key.End,        OnMoveCaret(CaretMovementType.DocumentEnd));
            AddBinding(EditingCommands.SelectToDocumentEnd,     Ctrl | Shift,   Key.End,        OnMoveCaretExtendSelection(CaretMovementType.DocumentEnd));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, OnSelectAll));

            TextAreaDefaultInputHandler.WorkaroundWPFMemoryLeak(InputBindings);
        }

        private static void OnSelectAll(object target, ExecutedRoutedEventArgs args)
        {
            TextArea textArea = GetTextArea(target);
            if (textArea != null && textArea.Document != null)
            {
                args.Handled = true;
                textArea.Caret.Offset = textArea.Document.TextLength;
                textArea.Selection = SimpleSelection.Create(textArea, 0, textArea.Document.TextLength);
            }
        }

        private static TextArea GetTextArea(object target)
        {
            return target as TextArea;
        }

        private enum CaretMovementType
        {
            CharLeft,
            CharRight,
            WordLeft,
            WordRight,
            LineUp,
            LineDown,
            PageUp,
            PageDown,
            LineStart,
            LineEnd,
            DocumentStart,
            DocumentEnd,
            MultyLeft,
            MultyUp,
            MultyRight,
            MultyDown,
        }

        private static ExecutedRoutedEventHandler OnMoveCaret(CaretMovementType direction)
        {
            return (target, args) =>
            {
                TextArea textArea = GetTextArea(target);
                if (textArea != null && textArea.Document != null)
                {
                    args.Handled = true;
                    textArea.ClearSelection();
                    MoveCaret(textArea, direction);
                    textArea.Caret.BringCaretToView();
                }
            };
        }

        private static ExecutedRoutedEventHandler OnMoveCaretExtendSelection(CaretMovementType direction)
        {
            return (target, args) =>
            {
                TextArea textArea = GetTextArea(target);
                if (textArea != null && textArea.Document != null)
                {
                    args.Handled = true;
                    var oldPosition = textArea.Caret.Position;
                    MoveCaret(textArea, direction);

                    if (direction < CaretMovementType.MultyLeft)
                        textArea.Selection = textArea.Selection.StartSelectionOrSetEndpoint(oldPosition, textArea.Caret.Position);
                    else
                    {
                        if (textArea.Selection is RectangleSelection)
                            textArea.Selection = textArea.Selection.StartSelectionOrSetEndpoint(oldPosition, textArea.Caret.Position);
                        else
                            textArea.Selection = new RectangleSelection(textArea, oldPosition, textArea.Caret.Position);
                    }

                    textArea.Caret.BringCaretToView();
                }
            };
        }

        #region Caret movement

        private static void MoveCaret(TextArea textArea, CaretMovementType direction)
        {
            var caretLine     = textArea.Document.GetLineByNumber(textArea.Caret.Line);
            var visualLine    = textArea.TextView.GetOrConstructVisualLine(caretLine);
            var caretPosition = textArea.Caret.Position;
            var textLine      = visualLine.GetTextLine(caretPosition.VisualColumn);

            switch (direction)
            {
                case CaretMovementType.CharLeft:
                    MoveCaretLeft(textArea, caretPosition, visualLine, CaretPositioningMode.Normal);
                    break;

                case CaretMovementType.CharRight:
                    MoveCaretRight(textArea, caretPosition, visualLine, CaretPositioningMode.Normal);
                    break;

                case CaretMovementType.WordLeft:
                    MoveCaretLeft(textArea, caretPosition, visualLine, CaretPositioningMode.WordStart);
                    break;

                case CaretMovementType.WordRight:
                    MoveCaretRight(textArea, caretPosition, visualLine, CaretPositioningMode.WordStart);
                    break;

                case CaretMovementType.LineUp:
                case CaretMovementType.LineDown:
                case CaretMovementType.PageUp:
                case CaretMovementType.PageDown:
                    MoveCaretUpDown(textArea, direction, visualLine, textLine, caretPosition.VisualColumn);
                    break;

                case CaretMovementType.DocumentStart:
                    SetCaretPosition(textArea, 0, 0);
                    break;

                case CaretMovementType.DocumentEnd:
                    SetCaretPosition(textArea, -1, textArea.Document.TextLength);
                    break;

                case CaretMovementType.LineStart:
                    MoveCaretToStartOfLine(textArea, visualLine);
                    break;

                case CaretMovementType.LineEnd:
                    MoveCaretToEndOfLine(textArea, visualLine);
                    break;

                case CaretMovementType.MultyLeft:
                    textArea.Caret.Position = new TextViewPosition(caretPosition.Line, caretPosition.Column-1);
                    textArea.Caret.DesiredXPos = double.NaN;
                    break;
                case CaretMovementType.MultyUp:
                    textArea.Caret.Position = new TextViewPosition(caretPosition.Line-1, caretPosition.Column);
                    textArea.Caret.DesiredXPos = double.NaN;
                    break;
                case CaretMovementType.MultyRight:
                    textArea.Caret.Position = new TextViewPosition(caretPosition.Line, caretPosition.Column+1);
                    textArea.Caret.DesiredXPos = double.NaN;
                    break;
                case CaretMovementType.MultyDown:
                    textArea.Caret.Position = new TextViewPosition(caretPosition.Line+1, caretPosition.Column);
                    textArea.Caret.DesiredXPos = double.NaN;
                    break;

                default:
                    throw new NotSupportedException(direction.ToString());
            }
        }

        #endregion Caret movement

        #region Home/End

        private static void MoveCaretToStartOfLine(TextArea textArea, VisualLine visualLine)
        {
            int newVC = visualLine.GetNextCaretPosition(-1, LogicalDirection.Forward, CaretPositioningMode.WordStart, textArea.Selection.EnableVirtualSpace);
            if (newVC < 0)
                throw ThrowUtil.NoValidCaretPosition();
            // when the caret is already at the start of the text, jump to start before whitespace
            if (newVC == textArea.Caret.VisualColumn)
                newVC = 0;
            int offset = visualLine.FirstDocumentLine.Offset + visualLine.GetRelativeOffset(newVC);
            SetCaretPosition(textArea, newVC, offset);
        }

        private static void MoveCaretToEndOfLine(TextArea textArea, VisualLine visualLine)
        {
            int newVC = visualLine.VisualLength;
            int offset = visualLine.FirstDocumentLine.Offset + visualLine.GetRelativeOffset(newVC);
            SetCaretPosition(textArea, newVC, offset);
        }

        #endregion Home/End

        #region By-character / By-word movement

        private static void MoveCaretRight(TextArea textArea, TextViewPosition caretPosition, VisualLine visualLine, CaretPositioningMode mode)
        {
            int pos = visualLine.GetNextCaretPosition(caretPosition.VisualColumn, LogicalDirection.Forward, mode, textArea.Selection.EnableVirtualSpace);
            if (pos >= 0)
            {
                SetCaretPosition(textArea, pos, visualLine.GetRelativeOffset(pos) + visualLine.FirstDocumentLine.Offset);
            }
            else
            {
                // move to start of next line
                DocumentLine nextDocumentLine = visualLine.LastDocumentLine.NextLine;
                if (nextDocumentLine != null)
                {
                    VisualLine nextLine = textArea.TextView.GetOrConstructVisualLine(nextDocumentLine);
                    pos = nextLine.GetNextCaretPosition(-1, LogicalDirection.Forward, mode, textArea.Selection.EnableVirtualSpace);
                    if (pos < 0)
                        throw ThrowUtil.NoValidCaretPosition();
                    SetCaretPosition(textArea, pos, nextLine.GetRelativeOffset(pos) + nextLine.FirstDocumentLine.Offset);
                }
                else
                {
                    // at end of document
                    Debug.Assert(visualLine.LastDocumentLine.Offset + visualLine.LastDocumentLine.TotalLength == textArea.Document.TextLength);
                    SetCaretPosition(textArea, -1, textArea.Document.TextLength);
                }
            }
        }

        private static void MoveCaretLeft(TextArea textArea, TextViewPosition caretPosition, VisualLine visualLine, CaretPositioningMode mode)
        {
            int pos = visualLine.GetNextCaretPosition(caretPosition.VisualColumn, LogicalDirection.Backward, mode, textArea.Selection.EnableVirtualSpace);
            if (pos >= 0)
            {
                SetCaretPosition(textArea, pos, visualLine.GetRelativeOffset(pos) + visualLine.FirstDocumentLine.Offset);
            }
            else
            {
                // move to end of previous line
                DocumentLine previousDocumentLine = visualLine.FirstDocumentLine.PreviousLine;
                if (previousDocumentLine != null)
                {
                    VisualLine previousLine = textArea.TextView.GetOrConstructVisualLine(previousDocumentLine);
                    pos = previousLine.GetNextCaretPosition(previousLine.VisualLength + 1, LogicalDirection.Backward, mode, textArea.Selection.EnableVirtualSpace);
                    if (pos < 0)
                        throw ThrowUtil.NoValidCaretPosition();
                    SetCaretPosition(textArea, pos, previousLine.GetRelativeOffset(pos) + previousLine.FirstDocumentLine.Offset);
                }
                else
                {
                    // at start of document
                    Debug.Assert(visualLine.FirstDocumentLine.Offset == 0);
                    SetCaretPosition(textArea, 0, 0);
                }
            }
        }

        #endregion By-character / By-word movement

        #region Line+Page up/down

        private static void MoveCaretUpDown(TextArea textArea, CaretMovementType direction, VisualLine visualLine, TextLine textLine, int caretVisualColumn)
        {
            // moving up/down happens using the desired visual X position
            double xPos = textArea.Caret.DesiredXPos;
            if (double.IsNaN(xPos))
                xPos = visualLine.GetTextLineVisualXPosition(textLine, caretVisualColumn);
            // now find the TextLine+VisualLine where the caret will end up in
            VisualLine targetVisualLine = visualLine;
            TextLine targetLine;
            int textLineIndex = visualLine.TextLines.IndexOf(textLine);
            switch (direction)
            {
                case CaretMovementType.LineUp:
                    {
                        // Move up: move to the previous TextLine in the same visual line
                        // or move to the last TextLine of the previous visual line
                        int prevLineNumber = visualLine.FirstDocumentLine.LineNumber - 1;
                        if (textLineIndex > 0)
                        {
                            targetLine = visualLine.TextLines[textLineIndex - 1];
                        }
                        else if (prevLineNumber >= 1)
                        {
                            DocumentLine prevLine = textArea.Document.GetLineByNumber(prevLineNumber);
                            targetVisualLine = textArea.TextView.GetOrConstructVisualLine(prevLine);
                            targetLine = targetVisualLine.TextLines[targetVisualLine.TextLines.Count - 1];
                        }
                        else
                        {
                            targetLine = null;
                        }
                        break;
                    }
                case CaretMovementType.LineDown:
                    {
                        // Move down: move to the next TextLine in the same visual line
                        // or move to the first TextLine of the next visual line
                        int nextLineNumber = visualLine.LastDocumentLine.LineNumber + 1;
                        if (textLineIndex < visualLine.TextLines.Count - 1)
                        {
                            targetLine = visualLine.TextLines[textLineIndex + 1];
                        }
                        else if (nextLineNumber <= textArea.Document.LineCount)
                        {
                            DocumentLine nextLine = textArea.Document.GetLineByNumber(nextLineNumber);
                            targetVisualLine = textArea.TextView.GetOrConstructVisualLine(nextLine);
                            targetLine = targetVisualLine.TextLines[0];
                        }
                        else
                        {
                            targetLine = null;
                        }
                        break;
                    }
                case CaretMovementType.PageUp:
                case CaretMovementType.PageDown:
                    {
                        // Page up/down: find the target line using its visual position
                        double yPos = visualLine.GetTextLineVisualYPosition(textLine, VisualYPosition.LineMiddle);
                        if (direction == CaretMovementType.PageUp)
                            yPos -= textArea.TextView.RenderSize.Height;
                        else
                            yPos += textArea.TextView.RenderSize.Height;
                        DocumentLine newLine = textArea.TextView.GetDocumentLineByVisualTop(yPos);
                        targetVisualLine = textArea.TextView.GetOrConstructVisualLine(newLine);
                        targetLine = targetVisualLine.GetTextLineByVisualYPosition(yPos);
                        break;
                    }
                default:
                    throw new NotSupportedException(direction.ToString());
            }
            if (targetLine != null)
            {
                double yPos = targetVisualLine.GetTextLineVisualYPosition(targetLine, VisualYPosition.LineMiddle);
                int newVisualColumn = targetVisualLine.GetVisualColumn(new Point(xPos, yPos), textArea.Selection.EnableVirtualSpace);
                SetCaretPosition(textArea, targetVisualLine, targetLine, newVisualColumn, false);
                textArea.Caret.DesiredXPos = xPos;
            }
        }

        #endregion Line+Page up/down

        #region SetCaretPosition

        private static void SetCaretPosition(TextArea textArea, VisualLine targetVisualLine, TextLine targetLine, int newVisualColumn, bool allowWrapToNextLine)
        {
            int targetLineStartCol = targetVisualLine.GetTextLineVisualStartColumn(targetLine);
            if (!allowWrapToNextLine && newVisualColumn >= targetLineStartCol + targetLine.Length)
            {
                if (newVisualColumn <= targetVisualLine.VisualLength)
                    newVisualColumn = targetLineStartCol + targetLine.Length - 1;
            }
            int newOffset = targetVisualLine.GetRelativeOffset(newVisualColumn) + targetVisualLine.FirstDocumentLine.Offset;
            SetCaretPosition(textArea, newVisualColumn, newOffset);
        }

        private static void SetCaretPosition(TextArea textArea, int newVisualColumn, int newOffset)
        {
            textArea.Caret.Position = new TextViewPosition(textArea.Document.GetLocation(newOffset), newVisualColumn);
            textArea.Caret.DesiredXPos = double.NaN;
        }

        #endregion SetCaretPosition
    }
}