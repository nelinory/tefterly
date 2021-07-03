using Serilog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace Tefterly.Core.Resources.Controls
{
    public partial class NoteEditor
    {
        // Hyperlink detection credit: https://blogs.msdn.microsoft.com/prajakta/2006/10/17/auto-detecting-hyperlinks-in-richtextbox-part-i/
        //                             https://blogs.msdn.microsoft.com/prajakta/2006/10/17/auto-detecting-hyperlinks-in-richtextbox-part-ii/

        private void ApplyHyperlinkFormat()
        {
            // Temporarily disable TextChanged event handler, since following code might insert hyperlink, which will raise another TextChanged event.
            TextChanged -= OnTextChanged;

            try
            {
                TextPointer caretPosition = Selection.Start;

                while (caretPosition != null && caretPosition.CompareTo(Selection.IsEmpty ? Selection.End.GetPositionAtOffset(0, LogicalDirection.Forward) : Selection.End) <= 0)
                {
                    TextRange wordRange = GetWordRange(caretPosition);
                    if (wordRange == null || wordRange.IsEmpty)
                        break;  // No more words in the document.

                    string word = Utilities.RemoveBulletsFromText(wordRange.Text);
                    if (IsValidUrl(word) && IsInHyperlinkScope(wordRange.Start) == false && IsInHyperlinkScope(wordRange.End) == false)
                    {
                        Hyperlink hyperlink = new Hyperlink(wordRange.Start, wordRange.End)
                        {
                            IsEnabled = true,
                            NavigateUri = new Uri(word),
                            Foreground = Utilities.GetColorBrushFromString(SettingsManager.Instance.Settings.Notes.HyperlinkForegroundColor)
                        };

                        hyperlink.RequestNavigate += OnRequestNavigate;
                        caretPosition = hyperlink.ElementEnd.GetNextInsertionPosition(LogicalDirection.Forward);
                    }
                    else
                        caretPosition = wordRange.End.GetNextInsertionPosition(LogicalDirection.Forward);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error while applying hyperlink format: {EX}", ex);
            }

            TextChanged += OnTextChanged;
        }

        private void RemoveHyperlinkFormat()
        {
            TextPointer caretPosition = Selection.Start;
            TextPointer backspacePosition = caretPosition.GetNextInsertionPosition(LogicalDirection.Backward);
            Hyperlink hyperlink = default(Hyperlink);

            try
            {
                if (backspacePosition != null && IsHyperlinkBoundaryCrossed(caretPosition, backspacePosition, ref hyperlink))
                {
                    // Remember caretPosition with forward gravity. This is necessary since we are going to delete 
                    // the hyperlink element preceding caretPosition and after deletion current caretPosition 
                    // (with backward gravity) will follow content preceding the hyperlink. 
                    // We want to remember content following the hyperlink to set new caret position at.

                    TextPointer newCaretPosition = caretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);

                    // 1. Copy its children Inline to a temporary array
                    InlineCollection hyperlinkChildren = hyperlink.Inlines;
                    Inline[] inlines = new Inline[hyperlinkChildren.Count];
                    hyperlinkChildren.CopyTo(inlines, 0);

                    // 2. Remove each child from parent hyperlink element and insert it after the hyperlink
                    for (int i = inlines.Length - 1; i >= 0; i--)
                    {
                        hyperlinkChildren.Remove(inlines[i]);
                        if (hyperlink.SiblingInlines != null)
                            hyperlink.SiblingInlines.InsertAfter(hyperlink, inlines[i]);
                    }

                    // 3. Apply hyperlink local formatting properties to inlines (which are now outside hyperlink scope)
                    LocalValueEnumerator localProperties = hyperlink.GetLocalValueEnumerator();
                    TextRange inlineRange = new TextRange(inlines[0].ContentStart, inlines[inlines.Length - 1].ContentEnd);

                    while (localProperties.MoveNext())
                    {
                        LocalValueEntry property = localProperties.Current;
                        DependencyProperty dependencyProperty = property.Property;
                        object value = property.Value;

                        // Ignore hyperlink defaults
                        if (dependencyProperty.ReadOnly == false
                            && dependencyProperty.Equals(Inline.TextDecorationsProperty) == false
                            && dependencyProperty.Equals(TextElement.ForegroundProperty) == false
                            && dependencyProperty.Equals(BaseUriHelper.BaseUriProperty) == false
                            && IsHyperlinkProperty(dependencyProperty) == false
                            && dependencyProperty.Name.Equals("IsEnabled") == false)
                        {
                            inlineRange.ApplyPropertyValue(dependencyProperty, value);
                        }
                    }

                    // 4. Delete the (empty) hyperlink element
                    if (hyperlink.SiblingInlines != null)
                    {
                        hyperlink.RequestNavigate -= OnRequestNavigate;
                        hyperlink.SiblingInlines.Remove(hyperlink);
                    }

                    // 5. Update selection, since we deleted Hyperlink element and caretPosition was at that hyperlink's end boundary
                    Selection.Select(newCaretPosition, newCaretPosition);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error while removing hyperlink format: {EX}", ex);
            }
        }

        private void SubscribeToAllHyperlinks(FlowDocument flowDocument)
        {
            var hyperlinks = Utilities.GetVisuals(flowDocument).OfType<Hyperlink>();
            foreach (var link in hyperlinks)
                link.RequestNavigate += OnRequestNavigate;
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // execute a hyperlink when clicked with Ctrl + MouseLeft
            Process.Start(new ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });

            e.Handled = true;
        }

        // Helper that parses a word as hyperlink
        private bool IsValidUrl(string word)
        {
            bool isValidUrl = false;

            Uri uriResult;
            if (Uri.TryCreate(word, UriKind.Absolute, out uriResult) == true)
            {
                if (uriResult.Scheme == Uri.UriSchemeHttps || uriResult.Scheme == Uri.UriSchemeHttp)
                    isValidUrl = true;
            }

            return isValidUrl;
        }

        // Helper that returns true if passed caretPosition and backspacePosition cross a hyperlink end boundary under the assumption that caretPosition and backSpacePosition are adjacent insertion positions
        private bool IsHyperlinkBoundaryCrossed(TextPointer caretPosition, TextPointer backspacePosition, ref Hyperlink backspacePositionHyperlink)
        {
            Hyperlink caretPositionHyperlink = GetHyperlinkAncestor(caretPosition);
            backspacePositionHyperlink = GetHyperlinkAncestor(backspacePosition);

            return (caretPositionHyperlink == null && backspacePositionHyperlink != null)
                || (caretPositionHyperlink != null && backspacePositionHyperlink != null && caretPositionHyperlink.Equals(backspacePositionHyperlink) == false);
        }

        // Helper that returns true when passed property applies to Hyperlink only.
        private bool IsHyperlinkProperty(DependencyProperty dependencyProperty)
        {
            return dependencyProperty.Equals(Hyperlink.CommandProperty)
                || dependencyProperty.Equals(Hyperlink.CommandParameterProperty)
                || dependencyProperty.Equals(Hyperlink.CommandTargetProperty)
                || dependencyProperty.Equals(Hyperlink.NavigateUriProperty)
                || dependencyProperty.Equals(Hyperlink.TargetNameProperty);
        }

        // Helper that returns true when passed TextPointer is within the scope of a Hyperlink element
        private bool IsInHyperlinkScope(TextPointer position)
        {
            return GetHyperlinkAncestor(position) != null;
        }

        /// <summary>
        /// Returns a TextRange covering a word containing or following this TextPointer
        /// </summary>
        /// <remarks>
        /// If this TextPointer is within a word or at start of word, the containing word range is returned
        /// If this TextPointer is between two words, the following word range is returned
        /// If this TextPointer is at trailing word boundary, the following word range is returned
        /// </remarks>
        private TextRange GetWordRange(TextPointer position)
        {
            TextRange wordRange = null;
            TextPointer wordStartPosition = null;

            // Go forward first, to find word end position
            TextPointer wordEndPosition = GetPositionAtWordBoundary(position, LogicalDirection.Forward);

            // Then travel backwards, to find word start position
            if (wordEndPosition != null)
                wordStartPosition = GetPositionAtWordBoundary(wordEndPosition, LogicalDirection.Backward);

            if (wordStartPosition != null && wordEndPosition != null)
                wordRange = new TextRange(wordStartPosition, wordEndPosition);

            return wordRange;
        }

        // Helper that returns a hyperlink ancestor of passed position
        private Hyperlink GetHyperlinkAncestor(TextPointer position)
        {
            Inline parent = position.Parent as Inline;
            while (parent != null && (parent is Hyperlink) == false)
                parent = parent.Parent as Inline;

            return parent as Hyperlink;
        }

        /// <summary>
        /// 1.  When wordBreakDirection = Forward, returns a position at the end of the word,
        ///     i.e. a position with a wordBreak character (space) following it
        /// 2.  When wordBreakDirection = Backward, returns a position at the start of the word,
        ///     i.e. a position with a wordBreak character (space) preceding it
        /// 3.  Returns null when there is no word break in the requested direction
        /// </summary>
        private TextPointer GetPositionAtWordBoundary(TextPointer position, LogicalDirection wordBreakDirection)
        {
            if (position.IsAtInsertionPosition == false)
                position = position.GetInsertionPosition(wordBreakDirection);

            TextPointer navigator = position;
            while (navigator != null && IsPositionNextToWordBreak(navigator, wordBreakDirection) == false)
                navigator = navigator.GetNextInsertionPosition(wordBreakDirection);

            return navigator;
        }

        // Helper for GetPositionAtWordBoundary
        // Returns true when passed TextPointer is next to a wordBreak in requested direction
        private bool IsPositionNextToWordBreak(TextPointer position, LogicalDirection wordBreakDirection)
        {
            bool isAtWordBoundary = false;

            // Skip over any formatting.
            if (position.GetPointerContext(wordBreakDirection) != TextPointerContext.Text)
                position = position.GetInsertionPosition(wordBreakDirection);

            if (position.GetPointerContext(wordBreakDirection) == TextPointerContext.Text)
            {
                LogicalDirection oppositeDirection = (wordBreakDirection == LogicalDirection.Forward) ? LogicalDirection.Backward : LogicalDirection.Forward;

                char[] runBuffer = new char[1];
                char[] oppositeRunBuffer = new char[1];

                position.GetTextInRun(wordBreakDirection, runBuffer, /*startIndex*/0, /*count*/1);
                position.GetTextInRun(oppositeDirection, oppositeRunBuffer, /*startIndex*/0, /*count*/1);

                if (runBuffer[0] == ' ' && (oppositeRunBuffer[0] == ' ') == false)
                    isAtWordBoundary = true;
            }
            else
            {
                // If we're not adjacent to text then we always want to consider this position a "word break".  
                // In practice, we're most likely next to an embedded object or a block boundary
                isAtWordBoundary = true;
            }

            return isAtWordBoundary;
        }
    }
}
