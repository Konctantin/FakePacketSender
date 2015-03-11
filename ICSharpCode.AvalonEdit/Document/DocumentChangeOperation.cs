﻿using System.Diagnostics;

namespace ICSharpCode.AvalonEdit.Document
{
    /// <summary>
    /// Describes a change to a TextDocument.
    /// </summary>
    internal sealed class DocumentChangeOperation : IUndoableOperationWithContext
    {
        private TextDocument document;
        private DocumentChangeEventArgs change;

        public DocumentChangeOperation(TextDocument document, DocumentChangeEventArgs change)
        {
            this.document = document;
            this.change = change;
        }

        public void Undo(UndoStack stack)
        {
            Debug.Assert(stack.state == UndoStack.StatePlayback);
            stack.RegisterAffectedDocument(document);
            stack.state = UndoStack.StatePlaybackModifyDocument;
            this.Undo();
            stack.state = UndoStack.StatePlayback;
        }

        public void Redo(UndoStack stack)
        {
            Debug.Assert(stack.state == UndoStack.StatePlayback);
            stack.RegisterAffectedDocument(document);
            stack.state = UndoStack.StatePlaybackModifyDocument;
            this.Redo();
            stack.state = UndoStack.StatePlayback;
        }

        public void Undo()
        {
            OffsetChangeMap map = change.OffsetChangeMapOrNull;
            document.Replace(change.Offset, change.InsertionLength, change.RemovedText, map != null ? map.Invert() : null);
        }

        public void Redo()
        {
            document.Replace(change.Offset, change.RemovalLength, change.InsertedText, change.OffsetChangeMapOrNull);
        }
    }
}