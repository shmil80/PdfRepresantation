using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace PdfRepresantation
{
    public class PdfImageWriter
    {
        readonly PdfShapeImageWriter shapeWriter;
        readonly PdfImageImageWriter imageWriter;
        readonly PdfLineImageWriter lineWriter;

        public PdfImageWriter()
        {
            shapeWriter = CreateShaperWriter();
            lineWriter = CreateLineWriter();
            imageWriter = CreateImageWriter();
        }

        protected virtual PdfShapeImageWriter CreateShaperWriter() => new PdfShapeImageWriter();
        protected virtual PdfLineImageWriter CreateLineWriter() => new PdfLineImageWriter();
        protected virtual PdfImageImageWriter CreateImageWriter() => new PdfImageImageWriter();

        public Bitmap[] ConvertToImages(PdfDetails pdf)
        {
            return pdf.Pages
                .Select(ConvertToImage)
                .ToArray();
        }

        public Bitmap ConvertToImage(PdfDetails pdf)
        {
            var width = pdf.Pages.Max(p => p.Width);
            var height = pdf.Pages.Sum(p => p.Height + 1) - 1;
            var bitmap = new Bitmap((int) width, (int) height);
            var graphics = Graphics.FromImage(bitmap);
            var penSeparator = new Pen(Color.Black, 1) {DashStyle = DashStyle.Dash};

            float top = 0;
            for (var index = 0; index < pdf.Pages.Count; index++)
            {
                var page = pdf.Pages[index];
                if (index > 0)
                {
                    graphics.DrawLine(penSeparator, 0, top, width, top);
                    top++;
                }

                Draw(graphics, page, top);
                top += page.Height;
            }

            return bitmap;
        }

        public Bitmap ConvertToImage(PdfPageDetails page)
        {
            var bitmap = new Bitmap((int) page.Width, (int) page.Height);
            var graphics = Graphics.FromImage(bitmap);
            Draw(graphics, page, 0);
            return bitmap;
        }

        public void Draw(Graphics graphics, PdfPageDetails page, float top)
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
                        imageWriter.DrawImage(graphics, page, image, top);
                        break;
                    case ShapeDetails shape:
                        shapeWriter.DrawShape(graphics, page, shape, top);
                        break;
                }
            }


            lineWriter.Init(graphics);
            foreach (var line in page.Lines)
            {
                lineWriter.DrawLine(graphics, page, line, top);
            }
        }

        public void SaveAsImage(PdfDetails details, string path)
        {
            var image = ConvertToImage(details);
            image.Save(path);
            //using (var stream = new FileStream(path,FileMode.Create))
            //{
            //    image.Save(stream, ImageFormat.Png);
            //}
        }
    }
}