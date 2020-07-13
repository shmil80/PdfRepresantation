using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace PdfRepresantation.Test
{
    //test the conversion
    //for those tests to run you need to put pdf files in the "File" direcory
    //and the result will be written in the "results" directory
    [TestClass]
    public class PdfRepresantationTest
    {
        private string sourceDir = "Files";
        private string targetDir = "Results";

        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            if (Directory.GetCurrentDirectory().Contains("netcoreapp"))
                Directory.SetCurrentDirectory(Path.Combine("..", "..", ".."));
            Log.logger = new ConsoleLogger {DebugSupported = true, InfoSupported = true, ErrorSupported = true};
        }

        [TestMethod]
        public void ConvertToHtml()
        {
            var paths = new List<string>();
            var htmlWriter = new PdfHtmlWriter(new HtmlWriterConfig { UseCanvas = true });
            foreach (var file in new DirectoryInfo(sourceDir).EnumerateFiles())
            {
                if(file.Extension!=".pdf")
                    continue;
                var name = Path.GetFileNameWithoutExtension(file.Name);
//                if(name!="addingImage")
//                    continue;
//                if(name!="pdf-020")
//                    continue;
                var details = PdfDetailsFactory.Create(file.FullName);
                var target = Path.Combine(targetDir, name + ".html");
                paths.Add(target);
                htmlWriter.SaveAsHtml(details, target);
            }

            var json = JsonConvert.SerializeObject(paths, Formatting.Indented);
            File.WriteAllText("urls.js", $"urls={json};");
        }

        [TestMethod]
        public void ConvertToText()
        {
            foreach (var file in new DirectoryInfo(sourceDir).EnumerateFiles())
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                //                if(name!="building")
                //                    continue;
                var details = PdfDetailsFactory.Create(file.FullName);
                var target = Path.Combine(targetDir, name + ".txt");
                File.WriteAllText(target, details.ToString());
            }
        }

        [TestMethod]
        public void ConvertToImage()
        {
            var imageWriter = new PdfImageWriter();
            foreach (var file in new DirectoryInfo(sourceDir).EnumerateFiles())
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                //                if(name!="building")
                //                    continue;
                var details = PdfDetailsFactory.Create(file.FullName);
                var target = Path.Combine(targetDir, name + ".png");

                imageWriter.SaveAsImage(details, target);
            }
        }
    }
}