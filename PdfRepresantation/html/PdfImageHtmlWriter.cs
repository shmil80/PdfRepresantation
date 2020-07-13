using System;
using System.IO;
using System.Text;

namespace PdfRepresantation
{
    public abstract class PdfImageHtmlWriter
    {
        private int indexImage = 1;
        protected readonly HtmlWriterConfig config;

        public PdfImageHtmlWriter(HtmlWriterConfig config)
        {
            this.config = config;
              if (config.EmbeddedImages && config.DirImages != null && Directory.Exists(config.DirImages))
                Directory.CreateDirectory(config.DirImages);
        }

        protected virtual void AssignPathImage(PdfPageDetails page, PdfImageDetails image, StringBuilder sb)
        {
            if (config.EmbeddedImages || config.DirImages == null)
            {
                sb.Append("data:image/png;base64, ")
                    .Append(Convert.ToBase64String(image.Buffer));
            }
            else
            {
                var file = new FileInfo(Path.Combine(config.DirImages, $"image-{page.PageNumber}-{indexImage++}.png"));
                var path = file.FullName;
                File.WriteAllBytes(path, image.Buffer);
                sb.Append(path);
            }
        }

        public abstract void AddImage(PdfPageDetails page, PdfImageDetails image, StringBuilder sb);

        public virtual void AddStyle(StringBuilder sb)
        {
        }
    }

   
    public class PdfImageHtmlTagWriter : PdfImageHtmlWriter
    {
        public PdfImageHtmlTagWriter(HtmlWriterConfig config) : base(config)
        {
        }

        public override void AddImage(PdfPageDetails page, PdfImageDetails image, StringBuilder sb)
        {
            sb.Append(@"
        <img class=""image"" height=""").Append(Math.Round(image.Height, config.RoundDigits))
                .Append("\" width=\"")
                .Append(Math.Round(image.Width, config.RoundDigits)).Append("\" src=\"");
            AssignPathImage(page, image, sb);

            sb.Append("\" style=\"right:").Append(Math.Round(image.Right, config.RoundDigits))
                .Append("px;left:").Append(Math.Round(image.Left, config.RoundDigits))
                .Append("px; top:").Append(Math.Round(image.Top, config.RoundDigits))
                .Append("px\">");
            ;
        }

        public override void AddStyle(StringBuilder sb)
        {
            sb.Append(@"
        .image{position:absolute;}");
        }
    }
    public class PdfImageHtmlCanvasWriter : PdfImageHtmlWriter
    {
        public PdfImageHtmlCanvasWriter(HtmlWriterConfig config) : base(config)
        {
        }

        public override void AddImage(PdfPageDetails page, PdfImageDetails image, StringBuilder sb)
        {
            var height = Math.Round(image.Height,config.RoundDigits);
            var width = Math.Round(image.Width,config.RoundDigits);
            sb.Append(@"
    </script>
        <img style=""display:none"" id=""image-").Append(image.Order)
                .Append("\" height=\"").Append(height)
                .Append("\" width=\"").Append(width)
                .Append("\" src=\"");
            AssignPathImage(page,image,sb);

            sb.Append(@"""/>
    <script>");
            sb.Append(@"
        drawImage('image-").Append(image.Order).Append("',")
                .Append(Math.Round(image.Left,config.RoundDigits)).Append(",").Append(Math.Round(image.Top,config.RoundDigits))
                .Append(",").Append(width).Append(",").Append(height).Append(");");

        }
    }

    public class PdfImageHtmlSvgWriter : PdfImageHtmlWriter
    {
        public PdfImageHtmlSvgWriter(HtmlWriterConfig config) : base(config)
        {
        }

        public override void AddImage(PdfPageDetails page, PdfImageDetails image, StringBuilder sb)
        {
            sb.Append(@"
        <image preserveAspectRatio=""none"" height=""").Append(Math.Round(image.Height, config.RoundDigits))
                .Append("\" width=\"")
                .Append(Math.Round(image.Width, config.RoundDigits)).Append("\" href=\"");
            AssignPathImage(page,image, sb);
            sb.Append("\" x=\"").Append(Math.Round(image.Left, config.RoundDigits))
                .Append("\" y=\"").Append(Math.Round(image.Top, config.RoundDigits)).Append("\"/>");
            ;
         
        }
    }
}