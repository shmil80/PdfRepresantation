using System.Drawing;

namespace PdfRepresantation
{
    public enum TextRenderOptions
    {
        Invisible = 0,
        Fill = 1,
        Stroke = 2,
        //TODO clip the path in the event "END_TEXT" via path.AddString method of system.drawing 
        Path = 4,
        Fill_Path = Fill | Path,
        Fill_Stroke = Fill | Stroke,
        Stroke_Path = Stroke | Path,
        Fill_Stroke_Path = Fill | Stroke | Path
    }

    public class TextRenderDetails
    {
        public TextRenderOptions Option { get; set; }
        public ColorDetails StrokeColor { get; set; }
        public ColorDetails FillColor { get; set; }
        public Color? MainColor { get; set; }
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this,obj)||
                   obj is TextRenderDetails other &&Equals(other);
        }

        bool Equals(TextRenderDetails other)
        {
            if (!MainColor.HasValue)
                return !other.MainColor.HasValue;
            if (!other.MainColor.HasValue)
                return false;
            var c = MainColor.Value;
            var o = other.MainColor.Value;
            return c.A == o.A && c.R == o.R && c.G == o.G && c.B == o.B;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                if (!MainColor.HasValue)
                    return -1;
                var c=MainColor.Value;
                
                var hashCode = (int)c.A;
                hashCode = (hashCode * 397) ^ c.R;
                hashCode = (hashCode * 397) ^ c.G;
                hashCode = (hashCode * 397) ^ c.B;
                return hashCode;
            }
        }
    }
}