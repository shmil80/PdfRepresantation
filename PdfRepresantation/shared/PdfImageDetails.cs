namespace PdfRepresantation
{
    public class PdfImageDetails : PdfDetailsItem,IPdfDrawingOrdered
    {
        public float? Rotation { get; set; }
        public int Order { get; set; }
        public byte[] Buffer { get; set; }
    }
}