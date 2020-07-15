using System;
using System.Collections;
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

        public static readonly FontManager Instance = new FontManager();

        private FontManager()
        {
        }

        public PdfFontDetails CreateFont(PdfFont pdfFont)
        {
            var fontProgram = pdfFont.GetFontProgram();
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
            font.Bold = fontProgram.GetFontNames().GetFontWeight() >= FontWeights.BOLD ||
                        fontName.Contains("bold") || fontName.Contains("Bold");
            font.Italic = fontName.Contains("italic") || fontName.Contains("Italic");
            font.Buffer = CreateBuffer(pdfFont);
            return font;
        }

        private byte[] CreateBuffer(PdfFont pdfFont)
        {
            //TODO the buffer of the font isn't completed
            return null;
            
            if (!pdfFont.IsEmbedded())
                return null;
            var fontProgram = pdfFont.GetFontProgram();
            var field = fontProgram.GetType()
                .GetField("fontFile", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
                return null;
            var stream = field.GetValue(fontProgram) as PdfStream;
            if (stream == null)
                return null;
            var buffer = stream.GetBytes();
//            var s = ((PdfDictionary) pdfFont.GetPdfObject())
//                .GetAsName(PdfName.Subtype).GetValue();
//            switch (s)
//            {
//                case "Type3":
//                case "Type1":
//                case "Type0":
//                case "TrueType":
//                    break;
//            }

            return buffer;
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