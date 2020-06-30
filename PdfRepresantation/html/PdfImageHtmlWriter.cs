using System;
using System.IO;
using System.Text;

namespace PdfRepresantation
{
    public class PdfImageHtmlWriter
    {
        private int indexImage = 1;
        private readonly bool embeddedImages;
        private readonly string dirImages;

        public PdfImageHtmlWriter(bool embeddedImages, string dirImages)
        {
            this.embeddedImages = embeddedImages;
            this.dirImages = dirImages;
            if (embeddedImages&&dirImages != null && Directory.Exists(dirImages))
                Directory.CreateDirectory(dirImages);

        }

        public virtual void AddImage(PdfPageDetails page, PdfImageDetails image, StringBuilder sb)
        {
            var wisth = Math.Round(image.Width,2);
            sb.Append(@"
        <img class=""image"" height=""").Append(Math.Round(image.Height,2))
                .Append("\" width=\"")
                .Append(Math.Round(image.Width,2)).Append("\" src=\"");
            if (embeddedImages || dirImages == null)
            {
                sb.Append("data:image/png;base64, ")
                    .Append(Convert.ToBase64String(image.Buffer));
            }
            else
            {
                string path;
                lock (this)
                {
                    FileInfo file;
                    do file = new FileInfo(Path.Combine(dirImages, "image" + indexImage++ + ".png"));
                    while (file.Exists);
                    path = file.FullName;
                    File.WriteAllBytes(path, image.Buffer);
                }

                sb.Append(path);
            }
            sb.Append("\" style=\"right:").Append(Math.Round( image.Right ,2))
                .Append( "px;left:").Append(Math.Round(image.Left,2))
                .Append("px; top:").Append(Math.Round(image.Top,2))
                .Append("px\">");
            ;
        }

        public virtual void AddStyle(StringBuilder sb)
        {
                sb.Append(@"
        .image{position:absolute;}");
        }
    }
}