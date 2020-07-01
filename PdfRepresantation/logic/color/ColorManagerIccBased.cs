using System;
using System.Drawing;
using System.Text;
using iText.Kernel.Pdf;

namespace PdfRepresantation
{
    public class ColorManagerIccBased : ColorManager
    {
        public static ColorManagerIccBased Instance = new ColorManagerIccBased();
        protected override ColorSpace Type => ColorSpace.IccBased;

        internal override Color? Color(iText.Kernel.Colors.Color colorPfd, float alpha)
        {
            var colorSpace = colorPfd.GetColorSpace();
            var source = ((PdfArray) colorSpace.GetPdfObject()).Get(1) as PdfDictionary;
            var alternateName = source.Get(PdfName.Alternate);
            ColorManager altManager = GetManagerBySpace(alternateName);
            if (altManager == null)
            {
                var length = source.GetAsInt(PdfName.N);
                altManager = NormalColorManager.GetManagerByLength(length);
            }

            var color = altManager?.Color(colorPfd, alpha);
            return color;
        }


        public override int LengthColor(PdfObject o)
        {
            var source = ((PdfArray) o).GetAsDictionary(1);
            var altName = source.Get(PdfName.Alternate);
            if (altName == null)
                return -1;
            var altManager = GetManagerBySpace(altName);
            return altManager?.LengthColor(altName) ?? -1;
        }
    }
}