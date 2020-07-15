using System.Collections.Generic;

namespace PdfRepresantation
{
    public class PdfTextLineDetails : PdfDetailsItem
    {
        public float? Rotation { get; set; }

        public IList<PdfTextResult> Texts { get; set; } = new List<PdfTextResult>();
        public string InnerText => string.Join("", Texts);
        public int[] Blocks { get; set; }

        public override string ToString() => InnerText + "\r\n";
    }
}