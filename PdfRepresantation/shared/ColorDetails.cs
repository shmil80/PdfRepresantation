using System.Collections.Generic;
using System.Drawing;
using iText.Kernel.Pdf.Colorspace;

namespace PdfRepresantation
{
    public abstract class ColorDetails
    {
        
    }

    public enum ColorSpace
    {
        DeviceGray,DeviceRGB,DeviceCMYK,
        Pattern,
        CalGray, CalRgb,Lab,
        IccBased,Indexed,Separation,DeviceN,

        
    }
    public class SimpleColorDetails:ColorDetails
    {
        public Color Color { get; set; }
        public ColorSpace Space { get; set; }
    }
    public class GardientColorDetails:ColorDetails
    {
        public GardientPoint Start { get; set; }
        public GardientPoint End { get; set; }
        public IList<ColorInGardient> Colors { get; set; }
    }
    public class ColorInGardient
    {
        public readonly Color Color;
        public readonly float? OffSet;

        public ColorInGardient(Color color, float? offSet)
        {
            Color = color;
            OffSet = offSet;
        }
    }
    public class GardientPoint
    {
        public float AbsoluteX { get; set; }
        public float AbsoluteY { get; set; }
        public float RelativeX { get; set; }
        public float RelativeY { get; set; }
    }
}