using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Function;

namespace PdfRepresantation
{
    class FunctionColorManager
    {
        public class FunctionColor
        {
            
        }
        public static FunctionColor GetFunctionDetails(PdfDictionary dict)
        {
            var function = new PdfFunction(dict);
            int[] domain;
            int[] encode;
            switch (function.GetFunctionType())
            {
                case 0:
                    domain = dict.GetAsArray(PdfName.Domain).ToIntArray();
                    dict.GetAsArray(PdfName.Size).ToIntArray();
                    dict.GetAsInt(PdfName.BitsPerSample);
                    dict.GetAsInt(PdfName.Order);
                    encode = dict.GetAsArray(PdfName.Encode).ToIntArray();
                    var decode = dict.GetAsArray(PdfName.Decode).ToIntArray();
                    var range = dict.GetAsArray(PdfName.Range).ToIntArray();
                    break;
                case 2:
                    domain = dict.GetAsArray(PdfName.Domain).ToIntArray();
                    var color1 = dict.GetAsArray(PdfName.C0).ToFloatArray();
                    var color2 = dict.GetAsArray(PdfName.C1).ToFloatArray();
                    var n = dict.GetAsInt(PdfName.N);
                    break;
                case 3:
                    domain = dict.GetAsArray(PdfName.Domain).ToIntArray();
                    foreach (PdfDictionary sub in dict.GetAsArray(PdfName.Functions))
                    {
                        GetFunctionDetails(sub);
                    }

                    var bounds = dict.GetAsArray(PdfName.Bounds).ToFloatArray();
                    encode = dict.GetAsArray(PdfName.Encode).ToIntArray();
                    break;
                case 4:
                    domain = dict.GetAsArray(PdfName.Domain).ToIntArray();
                    break;
            }

            return null;
        }

    }
}