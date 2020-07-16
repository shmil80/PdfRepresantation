using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace PdfRepresantation
{
    public class PdfHtmlWriter
    {
        protected readonly PdfDrawHtmlWriter drawWriter;
        protected readonly PdfHeaderHtmlWriter headerWriter;
        protected readonly PdfTextHtmlWriter textWriter;
        protected readonly PdfImageHtmlWriter imageWriter;
        protected readonly HtmlWriterConfig config;
        protected readonly PdfFontHtmlWriter fontWriter;

        public PdfHtmlWriter(HtmlWriterConfig config = null)
        {
            if (config == null)
                config = new HtmlWriterConfig();
            this.config = config;
            if (config.DrawShapes)
                drawWriter = CreateDrawWriter();
            if (config.AddHeader)
                headerWriter = CreateHeaderWriter();
            this.fontWriter = CreateFontWriter();
            textWriter = CreateTextWriter();
            imageWriter = CreateImageWriter();
        }

        protected virtual PdfHeaderHtmlWriter CreateHeaderWriter()
            => new PdfHeaderHtmlWriter(config);

        protected virtual PdfTextHtmlWriter CreateTextWriter()
            => new PdfTextHtmlWriter(config,fontWriter);

        protected virtual PdfImageHtmlWriter CreateImageWriter()
            => new PdfImageHtmlTagWriter(config);
        
        protected virtual PdfFontHtmlWriter CreateFontWriter() => 
            new PdfFontHtmlWriter(config);

        protected virtual PdfDrawHtmlWriter CreateDrawWriter()
        {
            if (config.UseCanvas)
                return new PdfDrawCanvasHtmlWriter(config);
            else
                return new PdfDrawSvgHtmlWriter(config);
        }


        public static void AppendColor(Color color, PdfHtmlWriterContext sb)
        {
            sb.Append("#").Append(color.R.ToString("X2"))
                .Append(color.G.ToString("X2"))
                .Append(color.B.ToString("X2"));
            if (color.A != 255)
                sb.Append(color.A.ToString("X2"));
        }


        public string ConvertPage(PdfPageDetails page,string location=null,string prefix=null)
        {
            var sb = new PdfHtmlWriterContext(prefix,location);
            sb.fontRef =fontWriter.CreateFontRef(page.Fonts);
            var allLines = page.Lines;
            StartHtml(sb, Title(page), page.RightToLeft, allLines);
            AddPage(page, sb);
            EndHtml(sb);
            return sb.Text;
        }

        public string ConvertPdf(PdfDetails pdf,string location=null,string prefix=null)
        {
            var sb = new PdfHtmlWriterContext(prefix,location);
            sb.fontRef = fontWriter.CreateFontRef(pdf.Fonts);
            var allLines = pdf.Pages.SelectMany(p => p.Lines);
            StartHtml(sb, Title(pdf), null, allLines);
            foreach (var page in pdf.Pages)
            {
                AddPage(page, sb);
            }

            EndHtml(sb);
            return sb.Text;
        }

        protected virtual string Title(PdfDetails pdf)
        {
            return "pdf converted to html";
        }

        protected virtual string Title(PdfPageDetails page)
        {
            return "pdf page " + page.PageNumber;
        }

      

        protected virtual void StartHtml(PdfHtmlWriterContext sb, string title, bool? rightToLeft,
             IEnumerable<PdfTextLineDetails> allLines)
        {
            sb.Append($@"<html>
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
    <meta name=""author"" content=""PdfRepresantation"" />
    <title>").Append(title).Append(@"</title>");
            AddStyle(allLines, sb);
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

        protected virtual void EndHtml(PdfHtmlWriterContext sb)
        {
            sb.Append(@"
</body>
</html>");
        }

        protected virtual void AddPage(PdfPageDetails page, PdfHtmlWriterContext sb)
        {
            headerWriter?.AddHeader(page, sb);
            var addedShapes = AddShapes(page, sb);
            var width = Math.Round(page.Width, config.RoundDigits);
            var height = Math.Round(page.Height, config.RoundDigits);
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
                textWriter.AddLine(page, line, sb);
            }

            sb.Append(@"
    </article>");
        }


        private bool AddShapes(PdfPageDetails page, PdfHtmlWriterContext sb)
        {
            if (page.Shapes.Count == 0)
                return false;
            if (drawWriter == null)
                return false;
            drawWriter.DrawShapesAndImages(page, sb);
            return true;
        }


        private void AddStyle(
            IEnumerable<PdfTextLineDetails> allLines,
            PdfHtmlWriterContext sb)
        {
            sb.Append(@"
    <style>");
            textWriter.AddTextStyle(sb);
            imageWriter.AddStyle(sb);
            drawWriter.AddStyle(sb);
            headerWriter?.AddStyle(sb);
            AddGlobalStyle(sb);
            fontWriter.AddFontStyle(allLines, sb);
            sb.Append(@"
    </style>");
        }

        protected virtual void AddGlobalStyle(PdfHtmlWriterContext sb)
        {
            sb.Append(@"
        .article{
            border-style: groove;
            position:relative;
            margin: 0 auto 0 auto;
            border-width: 2px;
        }");
        }


        public void SaveAsHtml(PdfDetails pdf, string path,string prefix=null)
        {
            var content = ConvertPdf(pdf,new FileInfo(path).Directory?.FullName,prefix);
            File.WriteAllText(path, content);
        }
    }
}