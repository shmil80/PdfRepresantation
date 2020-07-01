using System.Drawing;
using iText.Kernel.Pdf;

namespace PdfRepresantation
{
    public class ColorRgbManager : NormalColorManager
    {
        protected override ColorSpace Type { get; }
        public static ColorRgbManager CalRgbManager = new ColorRgbManager(ColorSpace.CalRgb);
        public static ColorRgbManager DeviceRGBManager = new ColorRgbManager(ColorSpace.DeviceRGB);
        public static ColorRgbManager LabManager = new ColorRgbManager(ColorSpace.Lab);

        private ColorRgbManager(ColorSpace type)
        {
            Type = type;
        }


        public static Color? FromArgb(float r, float g, float b, float alpha)
        {
            return System.Drawing.Color.FromArgb((int) (alpha * 255),
                (int) (r * 255),(int) (g * 255),(int) (b * 255));
        }

        public override Color? Color(float[] value, float alpha)
        {
            return FromArgb(value[0], value[1], value[2], alpha);
        }

        public override Color? Color(int[] value, float alpha)
        {
            return System.Drawing.Color.FromArgb((int) (alpha * 255),value[0], value[1], value[2]);
        }

        public override int LengthColor(PdfObject o) => 3;
    }
}