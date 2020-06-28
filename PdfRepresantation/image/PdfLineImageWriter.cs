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

        public void DrawLine(Graphics graphics, PdfPageDetails page, PdfTextLineDetails line, float top)
        {
            top += line.Top;
            var left = line.Left;
            var right = line.Right;
            foreach (var text in line.Texts)
            {
                Font font = CreateFont(text);
                Brush brush = new SolidBrush(text.Stroke.MainColor ?? Color.Black);
                float width;
                if (string.IsNullOrWhiteSpace(text.Value))
                {
                    var point = graphics.MeasureString(".", font, 0, StringFormat.GenericDefault).Width;
                    width = graphics.MeasureString("." + text.Value + ".", font, 0, StringFormat.GenericTypographic)
                                .Width - point * 2;
                }
                else
                    width = graphics.MeasureString(text.Value, font, 0, StringFormat.GenericTypographic).Width;

                if (page.RightToLeft)
                {
                    right += width;
                    graphics.DrawString(text.Value, font, brush, page.Width - right, top,
                        StringFormat.GenericTypographic);
                }
                else
                {
                    graphics.DrawString(text.Value, font, brush, left, top, StringFormat.GenericTypographic);
                    left += width;
                }
            }
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
            var font = new Font(text.Font.BasicFontFamily, text.FontSize - 1, style);
            return font;
        }
    }
}