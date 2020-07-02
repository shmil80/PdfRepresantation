using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
            if (embeddedImages && dirImages != null && Directory.Exists(dirImages))
                Directory.CreateDirectory(dirImages);
        }

        protected virtual void AddShapesAndImages(PdfPageDetails page, StringBuilder sb)
        {
            var items = new List<IPdfDrawingOrdered>();
            items.AddRange(page.Shapes);
            items.AddRange(page.Images);
            items.Sort((i1, i2) => i1.Order - i2.Order);
            var gradients = new Dictionary<GardientColorDetails, int>();
            for (int i = 0; i < page.Shapes.Count; i++)
            {
                var s = page.Shapes[i];
                if (s.FillColor is GardientColorDetails g)
                    gradients[g] = i;
            }

            if (gradients.Count > 0)
                InitGradients(gradients, sb);
            foreach (var item in items)
            {
                switch (item)
                {
                    case PdfImageDetails image:
                        AddImage(image, sb);
                        break;
                    case ShapeDetails shape:
                        AddShape(shape, sb, gradients);
                        break;
                }
            }
        }

        protected abstract void InitGradients(Dictionary<GardientColorDetails, int> gradients, StringBuilder sb);

        protected abstract void AddShape(ShapeDetails shape, StringBuilder sb,
            Dictionary<GardientColorDetails, int> gradients);

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

        public abstract void DrawShapesAndImages(PdfPageDetails page, StringBuilder sb);

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