using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PdfRepresantation.Test
{
    //test the service
    //for this test you need to run the server first

    //for those tests to run you need to put pdf files in the "File" direcory
    //and the result will be written in the "results" directory
    [TestClass]
    public class PdfRepresantationServiceTest
    {
        private string sourceDir = "Files";
        private string targetDir = "Results";
        private PdfRepresantationClient client = new PdfRepresantationClient();

        [TestMethod]
        public async Task ServerTestHtml()
        {
            foreach (var file in new DirectoryInfo(sourceDir).EnumerateFiles())
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                //if(name!="pdf-001")
                //    continue;
                var buffer = File.ReadAllBytes(file.FullName);
                var htmlResult = await client.ConvertToHtmlAsync(buffer);
                File.WriteAllText(Path.Combine(targetDir, name + ".html"), htmlResult);
            }
        }

        //for this test you need to run the server first
        [TestMethod]
        public async Task ServerTestText()
        {
            foreach (var file in new DirectoryInfo(sourceDir).EnumerateFiles())
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                //if(name!="pdf-001")
                //    continue;
                var buffer = File.ReadAllBytes(file.FullName);
                var textResult = await client.ConvertToTextAsync(buffer);
                File.WriteAllText(Path.Combine(targetDir, name + ".txt"), textResult);
            }
        }
    }
}