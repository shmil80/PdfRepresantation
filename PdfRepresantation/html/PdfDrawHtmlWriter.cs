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
        protected readonly HtmlWriterConfig config;

        protected PdfDrawHtmlWriter(HtmlWriterConfig config)
        {
            this.config = config;
            this.imageWriter = CreateImageWriter();
        }

        protected abstract PdfImageHtmlWriter CreateImageWriter();

        protected virtual void AddShapesAndImages(PdfPageDetails page, PdfHtmlWriterContext sb)
        {
            var gradients = new Dictionary<GardientColorDetails, int>();
            for (int i = 0; i < page.Shapes.Count; i++)
            {
                var s = page.Shapes[i];
                if (s.FillColor is GardientColorDetails g)
                    gradients[g] = i;
            }

            if (gradients.Count > 0)
                InitGradients(gradients, sb);
            foreach (var item in page.OrderedDawings)
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

        protected abstract void InitGradients(Dictionary<GardientColorDetails, int> gradients, PdfHtmlWriterContext sb);

        protected abstract void AddShape(ShapeDetails shape, PdfHtmlWriterContext sb,
            Dictionary<GardientColorDetails, int> gradients);


        public abstract void DrawShapesAndImages(PdfPageDetails page, PdfHtmlWriterContext sb);

        public abstract void AddScript(PdfHtmlWriterContext sb);


        public virtual void AddStyle(PdfHtmlWriterContext sb)
        {
            sb.Append(@"        
        .canvas{
            margin: 0 auto 0 auto;
            display: block;
        }");
        }
    }
}