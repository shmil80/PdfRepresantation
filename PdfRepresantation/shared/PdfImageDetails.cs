namespace PdfRepresantation
{
    public class PdfImageDetails : PdfDetailsItem,IPdfDrawingOrdered
    {
        public int Order { get; set; }
        public byte[] Buffer { get; set; }
    }
}