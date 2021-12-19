using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Tefterly.Core
{
    public static class PrintManager
    {
        public static void PrintNote(string documentTitle, FlowDocument sourceDocument)
        {
            FlowDocument printDocument = new FlowDocument();

            // clone original document, which prevents changes to the note window due to settings printing preferences
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Xaml format - no support for images
                // XamlPackage format - have issues with custom colors for hyperlinks
                TextRange sourceTextRange = new TextRange(sourceDocument.ContentStart, sourceDocument.ContentEnd);
                sourceTextRange.Save(memoryStream, DataFormats.Rtf);

                TextRange printTextRange = new TextRange(printDocument.ContentStart, printDocument.ContentEnd);
                printTextRange.Load(memoryStream, DataFormats.Rtf);
            }

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDocument.PageHeight = printDialog.PrintableAreaHeight;
                printDocument.PageWidth = printDialog.PrintableAreaWidth;
                printDocument.PagePadding = new Thickness(50);
                printDocument.ColumnGap = 0;
                printDocument.ColumnWidth = printDocument.PageWidth - (printDocument.PagePadding.Left + printDocument.PagePadding.Right);

                IDocumentPaginatorSource idocument = printDocument;

                printDialog.PrintDocument(idocument.DocumentPaginator, documentTitle);
            }
        }
    }
}
