using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using iText.IO.Font;
using PdfRepresantation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PdfRepresantation.Test
{
    [TestClass]
    public class TestPdfDecompressor
    {
        [TestMethod]
        [DataRow("bad-font")]
        public void Decompress(string name)
        {
            var file = $"Files/{name}.pdf";
            var result = $"Results/{name}.pdf";
            new PdfRepresantation.PdfDecompressor().Decompress(file,result);         
        }
    }
}