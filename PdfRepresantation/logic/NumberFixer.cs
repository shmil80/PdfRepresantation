using System.Collections.Generic;
using System.Linq;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;

namespace PdfRepresantation
{
    public class NumberFixer
    {
        private int StartOfMixOfNumbers(FontProgram fontProgram)
        {
            for (int i = 0; i < 10; i++)
            {
                var g = fontProgram.GetGlyphByCode(0x0013 + i);
                if (g != null)
                    return i;
            }

            return 10;
        }

        class RepairDetails
        {
            public char c;
            public int correctCid;
            public char unicode;
        }

        internal void ArrangeMixOfNumbers(FontProgram fontProgram, PdfFont pdfFont, byte[] buffer)
        {
            if (!(pdfFont is PdfType0Font))
                return;
            if (Enumerable.Range('0',10)
                .All(c=>fontProgram.GetGlyph(c)==null))
            {
                return;
            }
            

            var cmapRepair = new CmapDict(buffer);
            for (char c = '-'; c <= ':'; c++)
            {
                var correctCid = cmapRepair.GetCode(c);
                if (correctCid == 0)
                    continue;
                var g = fontProgram.GetGlyphByCode(correctCid);
                if (g == null)
                    continue;

                char unicode = (char) g.GetUnicode();
                if (unicode != c)
                {
                    g.SetChars(new[] {c});
                    g.SetUnicode(c);
                }
            }
        }

        internal void ArrangeMixOfNumbers(FontProgram fontProgram, PdfFont pdfFont)
        {
            if (NotInTheBugOfMixing(pdfFont))
                return;

            if (!IsMixOfNumbers(fontProgram))
                return;


            var startMix = StartOfMixOfNumbers(fontProgram);

            if (startMix == 0)
                startMix = -3; //include '-','.','/'
            var startCode = 0x0013 + startMix;
            var startCorrectChar = '0' + startMix;
            var length = '9' - '0' + 2 - startMix; //include after '9' - ':'

            for (int i = 0; i <= length; i++)
            {
                var g = fontProgram.GetGlyphByCode(startCode + i);
                if (g == null)
                    continue;
                char correct = (char) (startCorrectChar + i);

                var unicode = (char) g.GetUnicode();
                if (unicode != correct)
                {
                    g.SetChars(new[] {correct});
                    g.SetUnicode(correct);
                }
            }
        }

        private bool NotInTheBugOfMixing(PdfFont pdfFont)
        {
            if (!(pdfFont is PdfType0Font))
                return true;
            var arr = pdfFont.GetPdfObject().GetAsArray(PdfName.DescendantFonts);
            if (arr?.Count() != 1)
                return true;
            var des = arr.GetAsDictionary(0);
            var subType = des?.GetAsName(PdfName.Subtype);
            if (subType?.GetValue() != "CIDFontType2")
                return true;
            var cidtoGidMap = des.GetAsName(PdfName.CIDToGIDMap);
            if (cidtoGidMap?.GetValue() != "Identity")
                return true;
            var cidSystemInfo = des.GetAsDictionary(PdfName.CIDSystemInfo);
            var ordering = cidSystemInfo?.GetAsString(PdfName.Ordering);
            if (ordering?.GetValue() != "Identity")
                return true;
            var registry = cidSystemInfo.GetAsString(PdfName.Registry);
            if (registry?.GetValue() != "Adobe")
                return true;
            var filter = des.GetAsDictionary(PdfName.FontDescriptor)?
                .GetAsStream(PdfName.FontFile2)?
                .GetAsName(PdfName.Filter);
            if (filter?.GetValue() != "FlateDecode")
                return true;
            return false;
        }

        private static bool IsMixOfNumbers(FontProgram fontProgram)
        {
            int val0 = -1;
            bool val0Varry = false;
            for (int i = 0; i < 10; i++)
            {
                var g = fontProgram.GetGlyphByCode(0x0013 + i);
                if (g == null)
                    continue;
                var unicode = g.GetUnicode();
                if (unicode < ',' || unicode > ':')
                    return false;
                if (val0 == -1)
                    val0 = unicode - i;
                else if (val0 != unicode - i)
                    val0Varry = true;
            }

            return val0Varry;
        }

//        private static bool IsMixOfNumbers(FontProgram fontProgram)
//        {
//            int? val0 = null;
//            bool val0Varry = false;
//            for (char c = '0'; c <='9'; c++)
//            {
//                var g = fontProgram.GetGlyph(c);
//                if (g == null)
//                    continue;
//                var cid = g.GetCode();
////                if (unicode < ',' || unicode > 'z')
////                    return false;
//                if (val0==null)
//                    val0 = c - cid;
//                else if (val0.Value != c - cid)
//                    val0Varry = true;
//            }
//
//            return val0Varry;
//        }
    }
}