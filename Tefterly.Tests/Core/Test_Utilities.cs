using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Tefterly.Core;

namespace Tefterly.Tests
{
    [TestClass]
    public class Test_Utilities
    {
        private const string TEST_CONTENT = "Unit Test Content";

        [TestMethod]
        public void Test_GetVisuals()
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Hyperlink(new Run("Hyperlink #1")));
            paragraph.Inlines.Add(new Hyperlink(new Run("Hyperlink #2")));
            paragraph.Inlines.Add(new Hyperlink(new Run("Hyperlink #3")));

            FlowDocument flowDocument = new FlowDocument();
            flowDocument.Blocks.Add(paragraph);

            IEnumerable<Hyperlink> hyperlinks = Utilities.GetVisuals(flowDocument).OfType<Hyperlink>();
            Assert.AreEqual(3, hyperlinks.Count());
        }

        [TestMethod]
        public void Test_GetFlowDocumentFromText()
        {
            FlowDocument testFlowDocument = Utilities.GetFlowDocumentFromText(TEST_CONTENT);
            Assert.IsNotNull(testFlowDocument);
            
            TextRange textRange = new TextRange(testFlowDocument.ContentStart, testFlowDocument.ContentEnd);
            Assert.AreEqual(TEST_CONTENT, textRange.Text.Trim());
        }

        [TestMethod]
        public void Test_CloneFlowDocument()
        {
            Paragraph sourceParagraph = new Paragraph();
            sourceParagraph.Inlines.Add("Text #1");
            sourceParagraph.Inlines.Add("Text #2");

            FlowDocument sourceFlowDocument = new FlowDocument();
            sourceFlowDocument.Blocks.Add(sourceParagraph);

            FlowDocument cloneFlowDocument = Utilities.CloneFlowDocument(sourceFlowDocument);
            Assert.AreNotSame(sourceFlowDocument, cloneFlowDocument);

            Paragraph cloneParagraph = new Paragraph();
            cloneParagraph.Inlines.Add("Text #3");
            cloneFlowDocument.Blocks.Add(cloneParagraph);
            Assert.AreNotEqual(Utilities.GetTextFromFlowDocument(sourceFlowDocument), Utilities.GetTextFromFlowDocument(cloneFlowDocument));
        }
    }
}
