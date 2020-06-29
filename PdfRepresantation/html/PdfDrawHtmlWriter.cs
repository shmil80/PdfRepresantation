using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace PdfRepresantation
{
    public abstract class PdfDrawHtmlWriter
    {
        private bool embeddedImages;
        private string dirImages;

        protected PdfDrawHtmlWriter(bool embeddedImages, string dirImages)
        {
            this.embeddedImages = embeddedImages;
            this.dirImages = dirImages; 
            if (embeddedImages&&dirImages != null && Directory.Exists(dirImages))
                Directory.CreateDirectory(dirImages);

        }
        protected virtual void AddShapesAndImages(PdfPageDetails page, StringBuilder sb)
        {
            var items = new List<IPdfDrawingOrdered>();
            items.AddRange(page.Shapes);
            items.AddRange(page.Images);
            items.Sort((i1, i2) => i1.Order - i2.Order);
            foreach (var item in items)
            {
                switch (item)
                {
                    case PdfImageDetails image:
                        AddImage(image, sb);
                        break;
                    case ShapeDetails shape:
                        AddShape(shape, sb); 
                        break;
                }
            }
        }

        protected abstract void AddShape(ShapeDetails shape, StringBuilder sb);

        protected abstract void AddImage(PdfImageDetails image, StringBuilder sb);

        protected void AssignPathImage(PdfImageDetails image, StringBuilder sb)
        {
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
                    int index = image.Order;
                    do file = new FileInfo(Path.Combine(dirImages, "image" + index++ + ".png"));
                    while (file.Exists);
                    path = file.FullName;
                    File.WriteAllBytes(path, image.Buffer);
                }

                sb.Append(path);
            }
        }
        public abstract void AddShapes(PdfPageDetails page, StringBuilder sb);

        public abstract void AddScript(StringBuilder sb);


        public virtual void AddStyle(StringBuilder sb)
        {
            sb.Append(@"        
        .canvas{
            margin: 0 auto 0 auto;
            display: block;
        }");
       }
    }
}