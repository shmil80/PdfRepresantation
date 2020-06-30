using System;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using Color = System.Drawing.Color;

namespace PdfRepresantation
{
    public abstract class ColorManager
    {
        internal static ColorManager GetManagerByType(iText.Kernel.Colors.Color colorPfd)
        {
            switch (colorPfd)
            {
                case CalGray _: return ColorGrayManager.CalGrayManager;
                case DeviceGray _: return ColorGrayManager.DeviceGrayManager;
                case CalRgb _: return ColorRgbManager.CalRgbManager;
                case DeviceRgb _: return ColorRgbManager.DeviceRGBManager;
                case Lab _: return ColorRgbManager.LabManager;
                case DeviceCmyk _: return ColorCmykManager.Instance;
                case IccBased _: return ColorManagerIccBased.Instance;
                case PatternColor _: return ColorManagerPattern.Instance;
                case DeviceN _: return ColorManagerDeviceN.ManagerDeviceN;
                case Indexed _: return ColorManagerIndexed.Instance;
                case Separation _: return ColorManagerSeparation.ManagerSeparation;
                default: throw new IndexOutOfRangeException();
            }
        }

        internal ColorManager GetSpaceByName(PdfObject pdfObject)
        {
            if (pdfObject is PdfName name)
                return GetSpaceByName(name);
            if (pdfObject is PdfArray array)
                return GetSpaceByName(array.GetAsName(0));
            return null;

        }

        internal static ColorManager GetSpaceByName(PdfName name)
        {
            if (name.Equals(PdfName.CalGray)) return ColorGrayManager.CalGrayManager;
            if (name.Equals(PdfName.DeviceGray)) return ColorGrayManager.DeviceGrayManager;
            if (name.Equals(PdfName.CalRGB)) return ColorRgbManager.CalRgbManager;
            if (name.Equals(PdfName.DeviceRGB)) return ColorRgbManager.DeviceRGBManager;
            if (name.Equals(PdfName.Lab)) return ColorRgbManager.LabManager;
            if (name.Equals(PdfName.DeviceCMYK)) return ColorCmykManager.Instance;
            if (name.Equals(PdfName.ICCBased)) return ColorManagerIccBased.Instance;
            if (name.Equals(PdfName.Pattern)) return ColorManagerPattern.Instance;
            if (name.Equals(PdfName.DeviceN)) return ColorManagerDeviceN.ManagerDeviceN;
            if (name.Equals(PdfName.Indexed)) return ColorManagerIndexed.Instance;
            if (name.Equals(PdfName.Separation)) return ColorManagerSeparation.ManagerSeparation;
            throw new IndexOutOfRangeException();
        }

        protected abstract ColorSpace Type { get; }
        internal abstract Color? Color(iText.Kernel.Colors.Color colorPfd, float alpha);

        public static ColorDetails GetColor(iText.Kernel.Colors.Color colorPfd, float alpha)
        {
            var manager = GetManagerByType(colorPfd);
            return manager.GetColorDetails(colorPfd, alpha);
        }

        public virtual ColorDetails GetColorDetails(iText.Kernel.Colors.Color colorPfd, float alpha)
        {
            var color = Color(colorPfd, alpha);
            if (color == null)
                return null;
            return new SimpleColorDetails
            {
                Space = Type,
                Color = color.Value
            };
        }

        public abstract Color? Color(int[] value, float alpha);
        public abstract int LengthColor(PdfObject o);
    }
}