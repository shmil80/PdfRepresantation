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
        protected readonly PdfImageHtmlWriter imageWriter;

        protected PdfDrawHtmlWriter(bool embeddedImages, string dirImages)
        {
            this.imageWriter = CreateImageWriter(embeddedImages, dirImages);
        }

        protected abstract PdfImageHtmlWriter CreateImageWriter(bool embeddedImages, string dirImages);

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
                        imageWriter.AddImage(page, image, sb);
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