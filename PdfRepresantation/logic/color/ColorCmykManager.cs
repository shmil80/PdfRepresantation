using System.Drawing;
using iText.Kernel.Pdf;

namespace PdfRepresantation
{
    public class ColorCmykManager : NormalColorManager
    {
        protected override ColorSpace Type => ColorSpace.DeviceCMYK;
        public static ColorCmykManager Instance = new ColorCmykManager();


        public static Color FromCmyk(float c, float m, float y, float k, float alpha)
        {
            var r = (int) (255 * (1 - c) * (1 - k));
            var g = (int) (255 * (1 - m) * (1 - k));
            var b = (int) (255 * (1 - y) * (1 - k));
            return System.Drawing.Color.FromArgb((int) (alpha * 255), r, g, b);
        }


        public override Color? Color(float[] value, float alpha)
        {
            return FromCmyk(value[0], value[1], value[2], value[3], alpha);
        }

        public override Color? Color(int[] value, float alpha)
        {
            return FromCmyk(value[0] / 255F, value[1] / 255F, value[2] / 255F, value[3] / 255F, alpha);
        }

        public override int LengthColor(PdfObject o) => 4;
    }
}