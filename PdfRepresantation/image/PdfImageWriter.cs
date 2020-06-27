using System;
using System.Drawing;
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
            var bitmap = new Bitmap((int)pdf.Pages.Max(p => p.Width), (int)pdf.Pages.Sum(p => p.Height));
            var graphics = Graphics.FromImage(bitmap);
            float top = 0;
            foreach (var page in pdf.Pages)
            {
                Draw(graphics, page, top);
                top += page.Height;
            }

            return bitmap;
        }

        public Bitmap ConvertToImage(PdfPageDetails page)
        {
            var bitmap = new Bitmap((int)page.Width, (int)page.Height);
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
               lineWriter. DrawLine(graphics, page, line, top);
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