namespace PdfRepresantation
{
    public class HtmlWriterConfig
    {
        public bool DrawShapes { get; set; } = true;
        public bool UseEmbeddedFont { get; set; }
        public bool UseCanvas { get; set; }
        public bool AddHeader { get; set; }
        public int RoundDigits { get; set; } = 2;
    }
}