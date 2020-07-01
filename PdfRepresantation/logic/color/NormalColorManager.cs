using System.Drawing;

namespace PdfRepresantation
{
    public abstract class NormalColorManager : ColorManager
    {
        internal override Color? Color(iText.Kernel.Colors.Color colorPfd, float alpha)
        {
            var value = colorPfd.GetColorValue();
            return Color(value, alpha);
        } 
        public abstract Color? Color(float[] value, float alpha);
        public abstract Color? Color(int[] value, float alpha);
        public static NormalColorManager GetManagerByLength(int? length)
        {
            switch (length)
            {
                  case 1: return ColorGrayManager.DeviceGrayManager;
                  case 3: return ColorRgbManager.DeviceRGBManager;
                  case 4: return ColorCmykManager.Instance;
                  default: return null;
                       
            }
        }

    }
}