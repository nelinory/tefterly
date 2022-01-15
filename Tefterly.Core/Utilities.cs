using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

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
            string returnValue = inputText.Replace("•\t", String.Empty);
            returnValue = returnValue.Replace("•", String.Empty);

            return returnValue;
        }

        public static TextRange FormatFlowDocument(FlowDocument document)
        {
            TextRange textRange = new TextRange(document.ContentStart, document.ContentEnd);

            textRange.ApplyPropertyValue(TextElement.FontSizeProperty, SettingsManager.Instance.Settings.Notes.FontSize);
            textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily(SettingsManager.Instance.Settings.Notes.FontFamily));

            return textRange;
        }

        public static SolidColorBrush GetColorBrushFromString(string colorCode)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorCode));
        }

        public static void EnsureTargetFolderExists(string fileName)
        {
            string folderName = Path.GetDirectoryName(fileName);
            if (Directory.Exists(folderName) == false)
                Directory.CreateDirectory(folderName);
        }

        public static FlowDocument CloneFlowDocument(FlowDocument sourceDocument)
        {
            FlowDocument printDocument = new FlowDocument();

            // clone original document
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Xaml format - no support for images
                // XamlPackage format - have issues with custom colors for hyperlinks
                TextRange sourceTextRange = new TextRange(sourceDocument.ContentStart, sourceDocument.ContentEnd);
                sourceTextRange.Save(memoryStream, DataFormats.Rtf);

                TextRange printTextRange = new TextRange(printDocument.ContentStart, printDocument.ContentEnd);
                printTextRange.Load(memoryStream, DataFormats.Rtf);
            }

            return printDocument;
        }
    }
}
