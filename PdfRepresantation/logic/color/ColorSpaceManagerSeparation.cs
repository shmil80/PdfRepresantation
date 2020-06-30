using System;
using System.Drawing;
using iText.Kernel.Pdf;

namespace PdfRepresantation
{
    public class ColorManagerSeparation : ColorManager
    {
        public static ColorManagerSeparation ManagerSeparation=new ColorManagerSeparation();
        protected override ColorSpace Type => ColorSpace.Separation;
        internal override Color? Color(iText.Kernel.Colors.Color colorPfd, float alpha)
        {
            var colorSpace = colorPfd.GetColorSpace();
            var array = ((PdfArray) colorSpace.GetPdfObject());
            var baseName = array.Get(2);
            var baseManager = GetSpaceByName(baseName);
            var function = FunctionColorManager.GetFunctionDetails(array.GetAsDictionary(3));
            return null;

        }

        public override Color? Color(int[] value, float alpha)
        {
            return null;
        }

        public override int LengthColor(PdfObject o)
        {
            var dict = ((PdfArray) o);
            var baseName = dict.Get(2);
            var baseManager = GetSpaceByName(baseName);
            return baseManager.LengthColor(baseName);
        }
    }
}