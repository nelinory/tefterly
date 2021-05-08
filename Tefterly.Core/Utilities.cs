using System;
using System.IO;
using System.Windows.Documents;

namespace Tefterly.Core
{
    public static class Utilities
    {
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
                    textRange.Save(memoryStream, System.Windows.DataFormats.Text);
                    returnResult = RemoveBulletsFromText(textRange.Text);
                }
            }

            return returnResult;
        }

        // fix some bad looking bullet points in the plain text
        public static string RemoveBulletsFromText(string inputText)
        {
            string returnValue = String.Empty;

            returnValue = inputText.Replace("•\t", String.Empty);
            returnValue = returnValue.Replace("•", String.Empty);

            return returnValue;
        }
    }
}
