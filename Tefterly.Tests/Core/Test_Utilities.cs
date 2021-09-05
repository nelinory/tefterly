using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Documents;
using Tefterly.Core;

namespace Tefterly.Tests
{
    [TestClass]
    public class Test_Utilities
    {
        private const string TEST_CONTENT = "Unit Test Content";

        [TestMethod]
        public void Test_GetFlowDocumentFromText()
        {
            FlowDocument testFlowDocument = Utilities.GetFlowDocumentFromText(TEST_CONTENT);
            Assert.IsNotNull(testFlowDocument);
            
            TextRange textRange = new TextRange(testFlowDocument.ContentStart, testFlowDocument.ContentEnd);
            Assert.AreEqual(TEST_CONTENT, textRange.Text.Trim());
        }
    }
}
