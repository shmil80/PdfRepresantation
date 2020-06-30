using System.Drawing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Colorspace;

namespace PdfRepresantation
{
    public class ColorManagerIndexed : ColorManager
    {
        public static ColorManagerIndexed Instance=new ColorManagerIndexed();
        protected override ColorSpace Type => ColorSpace.Indexed;
        internal override Color? Color(iText.Kernel.Colors.Color colorPfd, float alpha)
        {
            var colorSpace = colorPfd.GetColorSpace();
            var dict = ((PdfArray) colorSpace.GetPdfObject());
            var baseName = dict.Get(0);
            var baseManager = GetSpaceByName(baseName);
            var lengthSpace = baseManager.LengthColor(baseName);
            var lookup = dict.Get(2);
            var index = (int)colorPfd.GetColorValue()[0];
            int[]value=new int[lengthSpace];
            if (lookup is PdfStream stream)
            {
                var bytes = stream.GetBytes();
                for (var i = 0; i < value.Length; i++)
                {
                    value[i] = bytes[index * lengthSpace + i];
                }               
            }
            else if (lookup is PdfArray array)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    value[i] = array.GetAsNumber(index * lengthSpace + i).IntValue();
                }
            }
            else return null;

            return baseManager.Color(value,alpha);

        }

        public override Color? Color(int[] value, float alpha)
        {
            return null;
        }

        public override int LengthColor(PdfObject o) => 1;
    }
}