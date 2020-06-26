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
        
    }
}