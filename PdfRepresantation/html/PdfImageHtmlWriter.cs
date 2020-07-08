using System;
using System.IO;
using System.Text;

namespace PdfRepresantation
{
    public abstract class PdfImageHtmlWriter
    {
        private int indexImage = 1;
        private readonly bool embeddedImages;
        private readonly string dirImages;

        public PdfImageHtmlWriter(bool embeddedImages, string dirImages)
        {
            this.embeddedImages = embeddedImages;
            this.dirImages = dirImages;
            if (embeddedImages && dirImages != null && Directory.Exists(dirImages))
                Directory.CreateDirectory(dirImages);
        }

        protected virtual void AssignPathImage(PdfPageDetails page, PdfImageDetails image, StringBuilder sb)
        {
            if (embeddedImages || dirImages == null)
            {
                sb.Append("data:image/png;base64, ")
                    .Append(Convert.ToBase64String(image.Buffer));
            }
            else
            {
                var file = new FileInfo(Path.Combine(dirImages, $"image-{page.PageNumber}-{indexImage++}.png"));
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
        public PdfImageHtmlTagWriter(bool embeddedImages, string dirImages) : base(embeddedImages, dirImages)
        {
        }


        public override void AddImage(PdfPageDetails page, PdfImageDetails image, StringBuilder sb)
        {
            sb.Append(@"
        <img class=""image"" height=""").Append(Math.Round(image.Height, 2))
                .Append("\" width=\"")
                .Append(Math.Round(image.Width, 2)).Append("\" src=\"");
            AssignPathImage(page, image, sb);

            sb.Append("\" style=\"right:").Append(Math.Round(image.Right, 2))
                .Append("px;left:").Append(Math.Round(image.Left, 2))
                .Append("px; top:").Append(Math.Round(image.Top, 2))
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
        public PdfImageHtmlCanvasWriter(bool embeddedImages, string dirImages) : base(embeddedImages, dirImages)
        {
        }

        public override void AddImage(PdfPageDetails page, PdfImageDetails image, StringBuilder sb)
        {
            sb.Append(@"
    </script>
        <img style=""display:none"" id=""image-").Append(image.Order)
                .Append("\" height=\"").Append(image.Height)
                .Append("\" width=\"")
                .Append(Math.Round(image.Width,2)).Append("\" src=\"");
            AssignPathImage(page,image,sb);

            sb.Append(@"""/>
    <script>");
            sb.Append(@"
        drawImage('image-").Append(image.Order).Append("',")
                .Append(Math.Round(image.Left,2)).Append(",").Append(Math.Round(image.Top,2))
                .Append(",").Append(Math.Round(image.Width,2)).Append(",").Append(Math.Round(image.Height,2)).Append(");");

        }
    }

    public class PdfImageHtmlSvgWriter : PdfImageHtmlWriter
    {
        public PdfImageHtmlSvgWriter(bool embeddedImages, string dirImages) : base(embeddedImages, dirImages)
        {
        }

        public override void AddImage(PdfPageDetails page, PdfImageDetails image, StringBuilder sb)
        {
            sb.Append(@"
        <image preserveAspectRatio=""none"" height=""").Append(Math.Round(image.Height, 2))
                .Append("\" width=\"")
                .Append(Math.Round(image.Width, 2)).Append("\" href=\"");
            AssignPathImage(page,image, sb);
            sb.Append("\" x=\"").Append(Math.Round(image.Left, 2))
                .Append("\" y=\"").Append(Math.Round(image.Top, 2)).Append("\"/>");
            ;
         
        }
    }
}