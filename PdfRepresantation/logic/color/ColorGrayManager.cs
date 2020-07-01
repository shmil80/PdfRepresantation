using System.Drawing;
using iText.Kernel.Pdf;

namespace PdfRepresantation
{
    public class ColorGrayManager : NormalColorManager
    {
        protected override ColorSpace Type { get; }
        public static ColorGrayManager CalGrayManager = new ColorGrayManager(ColorSpace.CalGray);
        public static ColorGrayManager DeviceGrayManager = new ColorGrayManager(ColorSpace.DeviceGray);

        private ColorGrayManager(ColorSpace type)
        {
            Type = type;
        }


        public static Color? FromGray(float gray, float alpha)
        {
            var g = (int) (gray * 255);
            return System.Drawing.Color.FromArgb((int) (alpha * 255),g,g,g);
        }

        public override Color? Color(float[] value, float alpha)
        {
            return FromGray(value[0], alpha);
        }

        public override Color? Color(int[] value, float alpha)
        {
            return System.Drawing.Color.FromArgb((int) (alpha * 255),value[0], value[0], value[0]);
        }

        public override int LengthColor(PdfObject o) => 1;
    }
}