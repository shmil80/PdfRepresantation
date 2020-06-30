using System.Drawing;
using iText.Kernel.Pdf;

namespace PdfRepresantation
{
    public class ColorManagerIccBased : ColorManager
    {
        public static ColorManagerIccBased Instance=new ColorManagerIccBased();
        protected override ColorSpace Type => ColorSpace.IccBased;
        internal override Color? Color(iText.Kernel.Colors.Color colorPfd, float alpha)
        {
            var colorSpace = colorPfd.GetColorSpace();
            var source = ((PdfArray) colorSpace.GetPdfObject()).Get(1) as PdfDictionary;
            var altName = source.Get(PdfName.Alternate);
            if (altName == null)
                return null;
            var altManager=GetSpaceByName(altName);
            var color = altManager?.Color(colorPfd, alpha);
            return color;
        }

        public override Color? Color(int[] value, float alpha)
        {
            return null;
        }

        public override int LengthColor(PdfObject o)
        {
            var source = ((PdfArray) o).GetAsDictionary(1);
            var altName = source.Get(PdfName.Alternate);
            if (altName == null)
                return -1;
            var altManager=GetSpaceByName(altName);
            return altManager?.LengthColor(altName)??-1;
        } 
    }
}