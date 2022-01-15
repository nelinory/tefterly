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
            FlowDocument printDocument = Utilities.CloneFlowDocument(sourceDocument);

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
