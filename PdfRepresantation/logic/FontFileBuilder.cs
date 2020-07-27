using System;
using System.Collections.Generic;
using iText.Kernel.Font;
using iText.Kernel.Pdf;

namespace PdfRepresantation
{
    public class FontFileBuilder
    {
        private readonly IDictionary<PdfStream, PdfFontFileDetails> cache =
            new Dictionary<PdfStream, PdfFontFileDetails>();
        internal bool ExtractFont(PdfFontDetails font, PdfFont pdfFont)
        {
            if (!pdfFont.IsEmbedded())
                return false;
            var fontObject = pdfFont.GetPdfObject();
            var fontDescriptor = GetDescriptor(fontObject);
            if (fontDescriptor == null)
                return false;

            var fontFile = ExtractFontFile(fontDescriptor, out var type);
            if (fontFile == null)
                return false;
            if (cache.TryGetValue(fontFile, out var fontFileDetails))
            {
                font.FontFile = fontFileDetails;
                return false;
            }

            fontFileDetails = new PdfFontFileDetails
            {
                HasUnicodeDictionary = fontObject.ContainsKey(PdfName.ToUnicode),
                FontType = type,
                Buffer = fontFile.GetBytes(),
                Name = "file-" + font.BasicFontFamily
            };

//            File.WriteAllBytes("font.ttf",fontFileDetails.Buffer);
            cache[fontFile] = fontFileDetails;
            font.FontFile = fontFileDetails;
            return true;



            //            var s = fontObject.GetAsName(PdfName.Subtype).GetValue();
//            switch (s)
//            {
//                case "Type3":
//                case "Type1":
//                case "Type0":
//                case "TrueType":
//                    break;
//            }
        }

        private static PdfStream ExtractFontFile(PdfDictionary fontDescriptor, out FontType type)
        {
            var fontFile = fontDescriptor.GetAsStream(PdfName.FontFile);
            if (fontFile != null)
            {
                type = FontType.Type1;
                return fontFile;
            }

            fontFile = fontDescriptor.GetAsStream(PdfName.FontFile2);
            if (fontFile != null)
            {
                type = FontType.TrueType;
                return fontFile;
            }

            fontFile = fontDescriptor.GetAsStream(PdfName.FontFile3);
            if (fontFile == null)
            {
                type = 0;
                return null;
            }

            switch (fontFile.GetAsName(PdfName.Subtype).GetValue())
            {
                case "Type1C":
                    type = FontType.CFF_Type1;
                    break;
                case "CIDFontType0C":
                    type = FontType.CFF_Type0;
                    break;
                case "OpenType":
                    type = FontType.CFF_OpenType;
                    break;
                default: throw new ArgumentException();
            }

            return fontFile;
        }
 
        private static PdfDictionary GetDescriptor(PdfDictionary fontObject)
        {
            var fontDescriptor = fontObject
                .GetAsDictionary(PdfName.FontDescriptor);
            if (fontDescriptor != null)
                return fontDescriptor;
            fontDescriptor = fontObject
                .GetAsArray(PdfName.DescendantFonts)?
                .GetAsDictionary(0)
                ?.GetAsDictionary(PdfName.FontDescriptor);

            return fontDescriptor;
        }

    }
}