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
            var htmlWriter = new PdfHtmlWriter(new HtmlWriterConfig {UseCanvas = false});
            foreach (var file in new DirectoryInfo(sourceDir).EnumerateFiles())
            {
                if (file.Extension != ".pdf")
                    continue;
                var name = Path.GetFileNameWithoutExtension(file.Name);
                switch (name)
                {
//                    case "d7663d8b1daad008f0d8d5316c8de685.mail-6":
//                    case "f583eabb4c8141a95d61aa593d148f26.mail-6":
//                    case "BF16C0C7D22765BF3ABDC457E60C0646":
//                    case "0a87227cd5b1e25950ab375d3454a2b4.mail-6":
//                    case "23337c607370bda9dbb0c2acea8b458d.mail-5":
//                    case "1d80a84db16f127c5db8c4ed329d0a2a.mail-7":
//                    case "3dbb4bac4a617e202be14d9904863184.mail-2":
//                    case "cd75fd06fbb27583c504e2c65a6f3b6c.mail-2":
//                    case "d64913c218e31d599a8cebaceebf1829.mail-7":
//                    case "03160500c630b6b75c93f1f7bae6b1aa.mail-6":
//                    case "3ef02310e24ca64c6924952dda3f77ea.mail-6":
//                    case "0c2a88620badc63eaa74c83e056bbdf1.mail-6":
//                    case "654f84af9b12c9779933c667c91b5b76.mail-4":
//                    case "860d723d1824259409de886fcb0f37db.mail-6":
//                    case "402525445ca18c4dbf9f6bc954b85464.mail-7":
//                    case "07b5e2cc5c94924926a0234942568ab7.mail-6":
                       case "CV-Eden Sabti":
                        break;
                    default:
                        continue;
                }

                var details = PdfDetailsFactory.Create(file.FullName);
                var target = Path.Combine(targetDir, name + ".html");
                paths.Add(target);
                htmlWriter.SaveAsHtml(details, target); //,"files/"+name+"-");
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