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
        [DataRow("b21175d0c48be889e1f055efe4a19e96.mail-6")]
        public void Decompress(string name)
        {
            var file = $"Files/{name}.pdf";
            var result = $"Results/{name}.pdf";
            new PdfRepresantation.PdfDecompressor().Decompress(file,result);         
        }
    }
}