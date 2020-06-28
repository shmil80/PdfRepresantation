using System;
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
            var height = pdf.Pages.Sum(p => p.Height+1)-1;
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
            foreach (var shape in page.Shapes)
            {
                shapeWriter.DrawShape(graphics, page, shape, top);
            }

            foreach (var image in page.Images)
            {
                imageWriter.DrawImage(graphics, page, image, top);
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