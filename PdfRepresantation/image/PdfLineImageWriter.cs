using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace PdfRepresantation
{
    public class PdfLineImageWriter
    {
        public virtual void Init(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
        }

        protected virtual StringFormat Format =>StringFormat.GenericTypographic;
        protected virtual float Measure(Graphics graphics, string s, Font font)
        {
            return graphics.MeasureString(s, font, 0, Format).Width ;
        }
        public void DrawLine(Graphics graphics, PdfPageDetails page, PdfTextLineDetails line, float top)
        {
            top += line.Top;
            var left = line.Left;
            var right = line.Right;
            if (line.Rotation != null)
            {
                graphics.TranslateTransform(left, line.Bottom);
                graphics.RotateTransform(line.Rotation.Value);
                graphics.TranslateTransform(-left, -line.Bottom);
            }
            foreach (var text in line.Texts)
            {
                Font font = CreateFont(text);
                Brush brush = new SolidBrush(text.Stroke.MainColor ?? Color.Black);
                float width;
                if (string.IsNullOrWhiteSpace(text.Value))
                {
                    var point = Measure(graphics, ".", font);
                    width = Measure(graphics, "." + text.Value + ".", font) - point * 2;
                }
                else
                    width =Measure(graphics,text.Value, font);

                if (page.RightToLeft)
                {
                    right += width;

                    graphics.DrawString(text.Value, font, brush, page.Width - right, top,
                        Format);
                }
                else
                {
                    graphics.DrawString(text.Value, font, brush, left, top, Format);
                    left += width;
                }
            }
            graphics.ResetTransform();
        }


        protected virtual Font CreateFont(PdfTextResult text)
        {
            FontStyle style;
            if (text.Font.Bold)
                if (text.Font.Italic)
                    style = FontStyle.Italic | FontStyle.Bold;
                else
                    style = FontStyle.Bold;
            else if (text.Font.Italic)
                style = FontStyle.Italic;
            else
                style = FontStyle.Regular;
            var font = new Font(text.Font.BasicFontFamily, text.FontSize, style,GraphicsUnit.Pixel);
            return font;
        }
    }
}