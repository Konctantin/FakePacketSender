using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Describes an entry in the <see cref="CompletionList"/>.
    /// </summary>
    public interface ICompletionData
    {
        /// <summary>
        /// Gets the image.
        /// </summary>
        ImageSource Image { get; }

        /// <summary>
        /// Gets the text. This property is used to filter the list of visible elements.
        /// </summary>
        string Name { get; set; }

        string RawData { get; set; }

        /// <summary>
        /// Gets the priority. This property is used in the selection logic. You can use it to prefer selecting those items
        /// which the user is accessing most frequently.
        /// </summary>
        double Priority { get; set; }

        /// <summary>
        /// The displayed content. This can be the same as 'Text', or a WPF UIElement if
        /// you want to display rich content.
        /// </summary>
        string Signature { get; set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        string Description { get; set; }

        List<ApiElement> ArgList { get; set; }

        List<ApiElement> RetList { get; set; }

        string Content { get; }

        /// <summary>
        /// Perform the completion.
        /// </summary>
        /// <param name="textArea">The text area on which completion is performed.</param>
        /// <param name="completionSegment">The text segment that was used by the completion window if
        /// the user types (segment between CompletionWindow.StartOffset and CompletionWindow.EndOffset).</param>
        /// <param name="insertionRequestEventArgs">The EventArgs used for the insertion request.
        /// These can be TextCompositionEventArgs, KeyEventArgs, MouseEventArgs, depending on how
        /// the insertion was triggered.</param>
        void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs);
    }
}