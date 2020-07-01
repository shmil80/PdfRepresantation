using System;
using System.Drawing;
using iText.Kernel.Pdf;

namespace PdfRepresantation
{
    public class ColorManagerSeparation : ColorManager
    {
        public static ColorManagerSeparation ManagerSeparation = new ColorManagerSeparation();
        protected override ColorSpace Type => ColorSpace.Separation;

        internal override Color? Color(iText.Kernel.Colors.Color colorPfd, float alpha)
        {
            var colorSpace = colorPfd.GetColorSpace();
            var array = ((PdfArray) colorSpace.GetPdfObject());
            var baseName = array.Get(2);
            var baseManager = (NormalColorManager) GetManagerBySpace(baseName);
            var function = Function.Create((PdfDictionary)array.Get(3));
            return baseManager.Color(function.Calculate(colorPfd.GetColorValue()), alpha);
        }


        public override int LengthColor(PdfObject o)
        {
            var dict = ((PdfArray) o);
            var baseName = dict.Get(2);
            var baseManager = GetManagerBySpace(baseName);
            return baseManager.LengthColor(baseName);
        }
    }
}