using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace PdfRepresantation
{
    public class PdfHtmlWriter
    {
        private readonly PdfDrawHtmlWriter drawWriter;

        private readonly PdfTextHtmlWriter textWriter;
        private readonly PdfImageHtmlWriter imageWriter;

        public PdfHtmlWriter(HtmlWriterConfig config = null)
        {
            if (config == null)
                config = new HtmlWriterConfig();

            if (config.DrawShapes)
                drawWriter = CreateShapeWriter(config);
            textWriter = CreateTextWriter();
            imageWriter = CreateImageWriter(config);
        }

        protected virtual PdfTextHtmlWriter CreateTextWriter()
            => new PdfTextHtmlWriter();

        protected virtual PdfImageHtmlWriter CreateImageWriter(HtmlWriterConfig config)
            => new PdfImageHtmlTagWriter(config.EmbeddedImages, config.DirImages);

        protected virtual PdfDrawHtmlWriter CreateShapeWriter(HtmlWriterConfig config)
        {
            if (config.UseCanvas)
                return new PdfDrawCanvasHtmlWriter(config.EmbeddedImages, config.DirImages);
            else
                return new PdfDrawSvgHtmlWriter(config.EmbeddedImages, config.DirImages);
        }


        public static void AppendColor(Color color, StringBuilder sb)
        {
            sb.Append("#").Append(color.R.ToString("X2"))
                .Append(color.G.ToString("X2"))
                .Append(color.B.ToString("X2"));
            if (color.A != 255)
                sb.Append(color.A.ToString("X2"));
        }


        public string ConvertPage(PdfPageDetails page)
        {
            var sb = new StringBuilder();
            var fontRef = CreateFontRef(page.Fonts);
            var allLines = page.Lines;
            StartHtml(sb, Title(page), page.RightToLeft, fontRef, allLines);
            AddPage(page, fontRef, sb);
            EndHtml(sb);
            return sb.ToString();
        }

        public string ConvertPdf(PdfDetails pdf)
        {
            var sb = new StringBuilder();
            var fontRef = CreateFontRef(pdf.Fonts);
            var allLines = pdf.Pages.SelectMany(p => p.Lines);
            StartHtml(sb, Title(pdf), null, fontRef, allLines);
            foreach (var page in pdf.Pages)
            {
                AddPage(page, fontRef, sb);
            }

            EndHtml(sb);
            return sb.ToString();
        }

        protected virtual string Title(PdfDetails pdf)
        {
            return "pdf converted to html";
        }

        protected virtual string Title(PdfPageDetails page)
        {
            return "pdf page " + page.PageNumber;
        }

        private static Dictionary<PdfFontDetails, int> CreateFontRef(IEnumerable<PdfFontDetails> fonts)
        {
            var fontRef = fonts
                .Select((f, i) => new {f, i})
                .ToDictionary(a => a.f, a => a.i);
            return fontRef;
        }

        protected virtual void StartHtml(StringBuilder sb, string title, bool? rightToLeft,
            Dictionary<PdfFontDetails, int> fontRef,
            IEnumerable<PdfTextLineDetails> allLines)
        {
            sb.Append($@"<html>
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
    <meta name=""author"" content=""PdfRepresantation"" />
    <title>").Append(title).Append(@"</title>");
            AddStyle(fontRef, allLines, sb);
            drawWriter.AddScript(sb);
            sb.Append(@"    
    <script>
        function init() {");
            textWriter.AddScriptInit(sb);
            sb.Append(@"
        }
    </script>");
            sb.Append($@"
</head>
<body onload=""init()""");
            if (rightToLeft.HasValue)
                sb.Append(@" dir=""").Append(rightToLeft.Value ? "rtl" : "ltr").Append("\"");
            sb.Append(">");
        }

        protected virtual void EndHtml(StringBuilder sb)
        {
            sb.Append(@"
</body>
</html>");
        }

        protected virtual void AddPage(PdfPageDetails page, Dictionary<PdfFontDetails, int> fontRef, StringBuilder sb)
        {
            AddHeader(page, sb);
            var addedShapes = AddShapes(page, sb);
            var width = Math.Round(page.Width,2);
            var height = Math.Round(page.Height,2);
            sb.Append(@"
    <article class=""article"" dir=""").Append(page.RightToLeft ? "rtl" : "ltr")
                .Append("\" style=\"width: ").Append(width)
                .Append("px;height:").Append(height)
                .Append("px;");
            if (addedShapes)
                sb.Append("margin-top:-").Append(height + 2)
                    .Append("px;");
            sb.Append("\">");
            if (!addedShapes)
            {
                foreach (var pdfImage in page.Images)
                {
                    imageWriter.AddImage(page, pdfImage, sb);
                }
            }
            foreach (var line in page.Lines)
            {
                textWriter.AddLine(page, fontRef, line, sb);
            }

            sb.Append(@"
    </article>");
        }

        protected virtual void AddHeader(PdfPageDetails page, StringBuilder sb)
        {
            sb.Append(@"
    <h2 class=""header"" style=""width: ").Append(Math.Round(page.Width,2))
                .Append("px;--text:'Page ").Append(page.PageNumber).Append("'\"></h2>");
        }


        private bool AddShapes(PdfPageDetails page, StringBuilder sb)
        {
            if (page.Shapes.Count == 0)
                return false;
            if (drawWriter == null)
                return false;
            drawWriter.DrawShapesAndImages(page, sb);
            return true;
        }


        private void AddStyle(Dictionary<PdfFontDetails, int> fontRef,
            IEnumerable<PdfTextLineDetails> allLines,
            StringBuilder sb)
        {
            sb.Append(@"
    <style>");
            textWriter.AddTextStyle(sb);
            imageWriter.AddStyle(sb);
			drawWriter.AddStyle(sb);
            AddGlobalStyle(sb);
            textWriter.AddFontStyle(fontRef, allLines, sb);
            sb.Append(@"
    </style>");
        }

        private static void AddGlobalStyle(StringBuilder sb)
        {
            sb.Append(@"
        .header{
            color: #795548;
            font-family: Arial;
            text-align: center;
            margin:20px auto 0 auto;
        }
        .header:before{content:var(--text);}
        .article{
            border-style: groove;
            position:relative;
            margin: 0 auto 0 auto;
            border-width: 2px;
        }");
        }


        public void SaveAsHtml(PdfDetails pdf, string path)
        {
            var content = ConvertPdf(pdf);
            File.WriteAllText(path, content);
        }
    }
}