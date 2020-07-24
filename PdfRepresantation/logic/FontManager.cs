using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.IO.Source;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Data;

namespace PdfRepresantation
{
    public class FontManager
    {
        readonly Regex fontFamilyRegex =
            new Regex(@"^.+\+|(PS|PSMT|MT|MS)$|((PS|PSMT|MT|MS)?[,-])?(Bold|Italic|MT|PS)+$",
                RegexOptions.ExplicitCapture);

        public readonly Dictionary<PdfFont, PdfFontDetails> fonts = new Dictionary<PdfFont, PdfFontDetails>();

        private readonly IDictionary<PdfStream, PdfFontFileDetails> cache =
            new Dictionary<PdfStream, PdfFontFileDetails>();

        public PdfFontDetails GetFont(PdfFont pdfFont)
        {
            if (!fonts.TryGetValue(pdfFont, out var font))
            {
                font = CreateFont(pdfFont);
                fonts.Add(pdfFont, font);
            }

            return font;
        }

        public PdfFontDetails CreateFont(PdfFont pdfFont)
        {
            var fontProgram = pdfFont.GetFontProgram();
            ArrangeMixOfNumbers(fontProgram, pdfFont);
            var font = new PdfFontDetails();
            var fontName
                = pdfFont.GetFontProgram().GetFontNames().GetFontName();
            if (fontName == null)
            {
                fontName = "Times New Roman";
            }

            font.FontFamily = fontName;
            var basicFontFamily = fontFamilyRegex.Replace(fontName, "");
            basicFontFamily = new string(SpaceInCamelCase(basicFontFamily).ToArray());
            font.BasicFontFamily = basicFontFamily.Trim();
            var lowerName = fontName.ToLowerInvariant();
            font.Bold = fontProgram.GetFontNames().GetFontWeight() >= FontWeights.BOLD ||
                        lowerName.Contains("bold");
            font.Italic = lowerName.Contains("italic");
            ExtractFont(font, pdfFont);
            return font;
        }

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

        private void ArrangeMixOfNumbers(FontProgram fontProgram, PdfFont pdfFont)
        {
            if (NotInTheBugOfMixing(pdfFont))
                return;
            if (!IsMixOfNumbers(fontProgram))
                return;

            var startMix = StartOfMixOfNumbers(fontProgram);
            int startCode, startCorrectChar, length;
            if (startMix == 0)
            {
                startCode = 0x0010;
                length = 14;
                startCorrectChar = '-';
            }
            else
            {
                startCode = 0x0013 + startMix;
                length = 11 - startMix;
                startCorrectChar = '0' + startMix;
            }

            for (int i = 0; i <= length; i++)
            {
                var g = fontProgram.GetGlyphByCode(startCode + i);
                if (g == null)
                    continue;
                char correct = (char) (startCorrectChar + i);
                var unicode = g.GetUnicode();
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
            var supplement = cidSystemInfo.GetAsNumber(PdfName.Supplement);
            if (supplement == null || supplement.IntValue() != 0)
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

        private void ExtractFont(PdfFontDetails font, PdfFont pdfFont)
        {
            if (!pdfFont.IsEmbedded())
                return;
            //TODO the buffer of the font isn't completed
            //return null;
            var fontObject = pdfFont.GetPdfObject();
            var fontDescriptor = GetDescriptor(fontObject);
            if (fontDescriptor == null)
                return;

            var fontFile = ExtractFontFile(fontDescriptor, out var type);
            if (fontFile == null)
                return;
            if (!cache.TryGetValue(fontFile, out var fontFileDetails))
            {
                fontFileDetails = new PdfFontFileDetails
                {
                    HasUnicodeDictionary = fontObject.ContainsKey(PdfName.ToUnicode),
                    FontType = type,
                    Buffer = fontFile.GetBytes(),
                    Name = "file-" + font.BasicFontFamily
                };
                cache[fontFile] = fontFileDetails;
            }

            font.FontFile = fontFileDetails;


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

        private IEnumerable<char> SpaceInCamelCase(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (i > 1 && char.IsUpper(c) && char.IsLower(s[i - 1]))
                    yield return ' ';
                yield return c;
            }
        }

        public float GetFontSize(TextRenderInfo textRenderInfo, PdfTextBlock text)
        {
            var fontSize = textRenderInfo.GetFontSize();
            var height = text.Height;
            if (fontSize > 0.99 && fontSize < 1.01)
            {
                LogWrongFontSize("no font size. take height of line:" + height);

                return height * 1.05F;
            }

            var ctm = textRenderInfo.GetGraphicsState().GetCtm();
            var yToY = ctm.Get(Matrix.I22);
            if (yToY > 0.99 && yToY < 1.01 && height > 0)
            {
                if (fontSize > height * 1.3)
                {
                    LogWrongFontSize("big fontSize:" + fontSize + ". take height of line:" + height);
                    return height * 1.3F;
                }
            }
            else
            {
                if (yToY > 0)
                {
                    fontSize *= yToY;
                    LogWrongFontSize("height font positive: " + yToY);
                }
                else
                    fontSize *= -yToY;

                if (fontSize > height * 2 || fontSize < height / 2)
                {
                    LogWrongFontSize("big fontSize:" + fontSize + ". take height of line:" + height);
                    return height;
                }
            }

            return fontSize;
        }

        private string lastLog;

        private void LogWrongFontSize(string m)
        {
            if (!Log.DebugSupported || lastLog == m)
                return;
            lastLog = m;
            Log.Debug(m);
        }
    }
}