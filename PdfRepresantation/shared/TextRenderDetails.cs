using System.Drawing;

namespace PdfRepresantation
{
    public enum TextRenderOptions
    {
        Invisible = 0,
        Fill = 1,
        Stroke = 2,
        Path = 4,
        Fill_Path = Fill | Path,
        Fill_Stroke = Fill | Stroke,
        Stroke_Path = Stroke | Path,
        Fill_Stroke_Path = Fill | Stroke | Path
    }

    public class TextRenderDetails
    {
        public TextRenderOptions Option { get; set; }
        public   ColorDetails StrokeColor{ get; set; }
        public   ColorDetails FillColor{ get; set; }
        public   Color? MainColor{ get; set; }
        
        
    }
}