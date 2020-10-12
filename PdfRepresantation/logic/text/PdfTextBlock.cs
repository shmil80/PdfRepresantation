using System.Collections.Generic;
using System.Drawing;

namespace PdfRepresantation
{
    public class PdfTextBlock
    {
        public bool? IsRightToLeft { get; set; }
        public float Left { get; set; }
        public float Width { get; set; }
        public float Right => Left + Width;
        public float Top => Bottom + Height;
        public float Height { get; set; }
        public float Bottom { get; set; }
        public float Start { get; set; }
        public float End { get; set; }
        public string Value { get; set; }
        public float FontSize { get; set; }
        public PdfFontDetails Font { get; set; }
        public TextRenderDetails StrokeColore { get; set; }
        public float SpaceWidth { get; set; }
        public float DistanceAfter { get; set; }
        public float BigSpace { get; set; }
        public string Link { get; set; }
        public float Rotation { get; set; }

        public int? Group { get; set; }
//        public bool IsDigit { get; set; }

        public override string ToString() => Value;

        private sealed class DistanceAfterRelationalComparer : IComparer<PdfTextBlock>
        {
            public int Compare(PdfTextBlock x, PdfTextBlock y) =>
                x == y ? 0 : y == null ? 1 : x == null ? -1 : x.DistanceAfter.CompareTo(y.DistanceAfter);
        }

        public static readonly IComparer<PdfTextBlock> DistanceAfterComparer = new DistanceAfterRelationalComparer();
    }
}