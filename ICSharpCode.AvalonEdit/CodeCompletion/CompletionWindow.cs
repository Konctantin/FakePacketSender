using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// The code completion window.
    /// </summary>
    public class CompletionWindow : CompletionWindowBase
    {
        private readonly CompletionList completionList = new CompletionList();
        private ToolTip toolTip = new ToolTip();
        private TextEditor textEditor;

        /// <summary>
        /// Gets the completion list used in this completion window.
        /// </summary>
        public CompletionList CompletionList
        {
            get { return completionList; }
        }

        public CompletionWindow(TextEditor textEditor)
            : this(textEditor.TextArea)
        {
            this.textEditor = textEditor;
            //toolTip.Style   = textEditor.Style;
        }

        /// <summary>
        /// Creates a new code completion window.
        /// </summary>
        public CompletionWindow(TextArea textArea)
            : base(textArea)
        {
            // keep height automatic
            this.CloseAutomatically = true;
            this.SizeToContent = SizeToContent.Height;
            this.MaxHeight = 300;
            this.Width = 250;
            this.Content = completionList;
            // prevent user from resizing window to 0x0
            this.MinHeight = 15;
            this.MinWidth = 30;

            #warning hack
            //this.Style = (Style)Application.Current.Resources.MergedDictionaries[0]["KamillaStyle"];

            toolTip.PlacementTarget = this;
            toolTip.Placement = PlacementMode.Right;
            toolTip.Closed += toolTip_Closed;

            AttachEvents();
        }

        #region ToolTip handling

        private void toolTip_Closed(object sender, RoutedEventArgs e)
        {
            // Clear content after tooltip is closed.
            // We cannot clear is immediately when setting IsOpen=false
            // because the tooltip uses an animation for closing.
            if (toolTip != null)
                toolTip.Content = null;
        }

        private void completionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (completionList.SelectedItem is ICompletionData)
            {
                toolTip.Content = completionList.SelectedItem.ToString();
                toolTip.IsOpen = true;
            }
            else
            {
                toolTip.IsOpen = false;
            }
        }

        #endregion ToolTip handling

        private void completionList_InsertionRequested(object sender, EventArgs e)
        {
            Close();
            // The window must close before Complete() is called.
            // If the Complete callback pushes stacked input handlers, we don't want to pop those when the CC window closes.
            var item = completionList.SelectedItem;
            if (item != null)
            {
                if (textEditor != null)
                {
                    item.Complete(this.TextArea, new AnchorSegment(this.TextArea.Document, this.textEditor.StartCurrentWord, this.textEditor.SelectionStart - this.textEditor.StartCurrentWord), e);
                }
                else
                {
                    item.Complete(this.TextArea, new AnchorSegment(this.TextArea.Document, this.StartOffset, this.EndOffset - this.StartOffset), e);
                }
            }
        }

        private void AttachEvents()
        {
            this.completionList.InsertionRequested += completionList_InsertionRequested;
            this.completionList.SelectionChanged += completionList_SelectionChanged;
            this.TextArea.Caret.PositionChanged += CaretPositionChanged;
            this.TextArea.MouseWheel += textArea_MouseWheel;
            this.TextArea.PreviewTextInput += textArea_PreviewTextInput;
        }

        /// <inheritdoc/>
        protected override void DetachEvents()
        {
            this.completionList.InsertionRequested -= completionList_InsertionRequested;
            this.completionList.SelectionChanged -= completionList_SelectionChanged;
            this.TextArea.Caret.PositionChanged -= CaretPositionChanged;
            this.TextArea.MouseWheel -= textArea_MouseWheel;
            this.TextArea.PreviewTextInput -= textArea_PreviewTextInput;
            base.DetachEvents();
        }

        /// <inheritdoc/>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (toolTip != null)
            {
                toolTip.IsOpen = false;
                toolTip = null;
            }
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled)
            {
                completionList.HandleKey(e);
            }
        }

        private void textArea_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = RaiseEventPair(this, PreviewTextInputEvent, TextInputEvent,
                                       new TextCompositionEventArgs(e.Device, e.TextComposition));
        }

        private void textArea_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = RaiseEventPair(GetScrollEventTarget(),
                                       PreviewMouseWheelEvent, MouseWheelEvent,
                                       new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta));
        }

        private UIElement GetScrollEventTarget()
        {
            if (completionList == null)
                return this;
            return completionList.ScrollViewer ?? completionList.ListBox ?? (UIElement)completionList;
        }

        /// <summary>
        /// Gets/Sets whether the completion window should close automatically.
        /// The default value is true.
        /// </summary>
        public bool CloseAutomatically { get; set; }

        /// <inheritdoc/>
        protected override bool CloseOnFocusLost
        {
            get { return this.CloseAutomatically; }
        }

        /// <summary>
        /// When this flag is set, code completion closes if the caret moves to the
        /// beginning of the allowed range. This is useful in Ctrl+Space and "complete when typing",
        /// but not in dot-completion.
        /// Has no effect if CloseAutomatically is false.
        /// </summary>
        public bool CloseWhenCaretAtBeginning { get; set; }

        private void CaretPositionChanged(object sender, EventArgs e)
        {
            int offset = this.TextArea.Caret.Offset;
            if (offset == this.StartOffset)
            {
                if (CloseAutomatically && CloseWhenCaretAtBeginning)
                {
                    Close();
                }
                else
                {
                    completionList.SelectItem(string.Empty);
                }
                return;
            }
            else
            {
                TextDocument document = this.TextArea.Document;
                if (document != null)
                {
                    completionList.SelectItem(textEditor.CurrentWordToCursor);
                }
            }
        }
    }
}