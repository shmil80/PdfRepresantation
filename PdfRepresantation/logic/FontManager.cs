using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using iText.IO.Font.Constants;
using iText.IO.Source;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser.Data;

namespace PdfRepresantation
{
    public class FontManager
    {
        readonly NumberFixer numberFixer = new NumberFixer();
        readonly FontFileBuilder fileBuilder = new FontFileBuilder();

        readonly Regex fontFamilyRegex =
            new Regex(@"^.+\+|(PS|PSMT|MT|MS)$|((PS|PSMT|MT|MS)?[,-])?(Bold|Italic|MT|PS)+$",
                RegexOptions.ExplicitCapture);

        public readonly Dictionary<PdfFont, PdfFontDetails> fonts = new Dictionary<PdfFont, PdfFontDetails>();


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
            var font = new PdfFontDetails();
            var fontName = pdfFont.GetFontProgram()
                .GetFontNames().GetFontName();
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
            fileBuilder.ExtractFont(font, pdfFont);
            numberFixer.ArrangeMixOfNumbers(fontProgram, pdfFont, font.FontFile?.Buffer);
            return font;
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


            var ctm = textRenderInfo.GetGraphicsState().GetCtm();
            var ctm2= textRenderInfo.GetTextMatrix();
            var yToY = ctm.Get(Matrix.I22)*ctm2.Get(Matrix.I22);
            if (yToY <= 0.99 || yToY >= 1.01 || height < 0)
            {
                if (yToY > 0)
                {
                    fontSize *= yToY;
                    LogWrongFontSize("height font positive: " + yToY);
                }
                else
                    fontSize *= -yToY;

//                if (fontSize > height * 2 || fontSize < height / 2)
//                {
//                    LogWrongFontSize("big fontSize:" + fontSize + ". take height of line:" + height);
//                    return height;
//                }
            }
            else
            {
//                if (fontSize > height * 1.5)
//                {
//                    LogWrongFontSize("big fontSize:" + fontSize + ". take height of line:" + height);
//                    return height * 1.3F;
//                }
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