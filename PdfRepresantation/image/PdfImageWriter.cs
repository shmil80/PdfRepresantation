using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace PdfRepresantation
{
    public class PdfImageWriter
    {
        public Bitmap[] ConvertToImage(PdfDetails pdf)
        {
            return pdf.Pages
                .Select(ConvertToImage)
                .ToArray();
        }
        public Bitmap ConvertToImage(PdfPageDetails page)
        {
            var bitmap = new Bitmap((int)page.Width,(int)page.Height);
            var graphics=Graphics.FromImage(bitmap);
            foreach (var shape in page.Shapes)
            {
                DrawShape(graphics, page, shape);
            }
            foreach (var image in page.Images)
            {
                DrawImage(graphics, page, image);
            }
            foreach (var line in page.Lines)
            {
                DrawLine(graphics, page, line);
            }
            return bitmap;
        }

        private void DrawLine(Graphics graphics, PdfPageDetails page, PdfTextLineDetails line)
        {
            throw new System.NotImplementedException();
        }

        private void DrawImage(Graphics graphics, PdfPageDetails page, PdfImageDetails image)
        {
            Image bitmap = Bitmap.FromStream(new MemoryStream(image.Buffer));
            RectangleF rect = new RectangleF(image.Left, image.Top, image.Width, image.Height);
            graphics.DrawImage(bitmap, rect);
        }

        private void DrawShape(Graphics graphics, PdfPageDetails page, ShapeDetails shape)
        {
            GraphicsPath path = new GraphicsPath();
            if (shape.EvenOddRule)
                path.FillMode = FillMode.Alternate;
            else
                path.FillMode = FillMode.Winding;
            var simpleColor = shape.FillColor as SimpleColorDetails;
            Brush brush = new SolidBrush(simpleColor.Color);
            graphics.FillPath(brush, path);
        }
    }
}