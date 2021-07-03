using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace Tefterly.Core
{
    public static class PrintManager
    {
        public static void PrintNote(string documentTitle, FlowDocument flowDocument)
        {
            FlowDocument printDocument;

            // clone original document, which prevents changes to the note window due to settings printing preferences
            using (StringReader stringReader = new StringReader(XamlWriter.Save(flowDocument)))
            {
                using (XmlReader xmlReader = XmlReader.Create(stringReader))
                {
                    printDocument = XamlReader.Load(xmlReader) as FlowDocument;
                }
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
