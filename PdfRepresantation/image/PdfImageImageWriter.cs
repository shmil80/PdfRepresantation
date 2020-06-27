using System.Drawing;
using System.IO;

namespace PdfRepresantation
{
    public class PdfImageImageWriter
    {


        public void DrawImage(Graphics graphics, PdfPageDetails page, PdfImageDetails image, float top)
        {
            Image bitmap = Bitmap.FromStream(new MemoryStream(image.Buffer));
            RectangleF rect = new RectangleF(image.Left, top + image.Top, image.Width, image.Height);
            graphics.DrawImage(bitmap, rect);
        }

    }
}