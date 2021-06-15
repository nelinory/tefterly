using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace Tefterly.Core
{
    public static class Utilities
    {
        public static IEnumerable<DependencyObject> GetVisuals(DependencyObject root)
        {
            foreach (DependencyObject child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            {
                yield return child;
                foreach (DependencyObject descendants in GetVisuals(child))
                    yield return descendants;
            }
        }

        public static FlowDocument GetFlowDocumentFromText(string text)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(text);

            return new FlowDocument(paragraph);
        }

        public static string GetTextFromFlowDocument(FlowDocument document)
        {
            string returnResult = String.Empty;

            if (document != null)
            {
                TextRange textRange = new TextRange(document.ContentStart, document.ContentEnd);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    textRange.Save(memoryStream, DataFormats.Text);
                    returnResult = ConvertBulletsInText(textRange.Text);
                }
            }

            return returnResult;
        }

        // fix some bad looking bullet points in the plain text
        public static string ConvertBulletsInText(string inputText)
        {
            return inputText.Replace("•\t", "  •  ");
        }

        public static string RemoveBulletsFromText(string inputText)
        {
            string returnValue = String.Empty;

            returnValue = inputText.Replace("•\t", String.Empty);
            returnValue = returnValue.Replace("•", String.Empty);

            return returnValue;
        }

        public static TextRange FormatFlowDocument(FlowDocument document)
        {
            TextRange textRange = new TextRange(document.ContentStart, document.ContentEnd);

            // TODO: Read this from settings
            textRange.ApplyPropertyValue(TextElement.FontSizeProperty, (Double)14);
            textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, new System.Windows.Media.FontFamily("Segoe UI"));

            return textRange;
        }
    }
}
