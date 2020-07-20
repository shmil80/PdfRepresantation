using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace PdfRepresantation
{
    public class PdfFontHtmlWriter
    {
        protected readonly HtmlWriterConfig config;
        NumberFormatInfo formatNumInClassName = new NumberFormatInfo {NumberDecimalSeparator = "-"};

        public PdfFontHtmlWriter(HtmlWriterConfig config)
        {
            this.config = config;
        }

        public virtual Dictionary<PdfFontDetails, int> CreateFontRef(IEnumerable<PdfFontDetails> fonts)
        {
            var fontRef = fonts
                .Select((f, i) => new {f, i})
                .ToDictionary(a => a.f, a => a.i);
            return fontRef;
        }

        public virtual void AddFontStyle(IEnumerable<PdfTextLineDetails> allLines,
            PdfHtmlWriterContext sb)
        {
            foreach (var size in allLines
                .SelectMany(l => l.Texts)
                .Select(t => Math.Round(t.FontSize * 2))
                .Distinct())
            {
                sb.Append(@"
        .font-size-").Append((size / 2).ToString(formatNumInClassName))
                    .Append("{font-size:").Append(size / 2).Append("px;}");
            }

            sb.Append(@"
        .bold{font-weight: bold;}");

            foreach (var pair in sb.fontRef)
            {
                bool created = CreateFont(sb, pair.Key.FontFile, pair.Value);


                sb.Append(@"
        .font").Append(pair.Value + 1).Append("{font-family:\"");
                if (created) sb.Append(pair.Key.FontFile.Name).Append("\",\"");
                sb.Append(pair.Key.FontFamily)
                    .Append("\",\"").Append(pair.Key.BasicFontFamily).Append("\"; ");
                if (pair.Key.Bold)
                    sb.Append(" font-weight: bold;");
                if (pair.Key.Italic)
                    sb.Append(" font-style: italic;");
                sb.Append('}');
            }
        }

        protected virtual bool CreateFont(PdfHtmlWriterContext sb, PdfFontFileDetails font, int index)
        {
            if (font == null)
                return false;
            if (!config.UseEmbeddedFont)
                return false;
            if (font.FontType != FontType.TrueType && font.FontType != FontType.TrueType)
                return false;
            if (sb.CreatedFont.Contains(font.Name))
                return true;
            sb.Append(@"
        @font-face { 
            font-family: '").Append(font.Name).Append(@"';
            src: url(");
            if (sb.Prefix == null)
            {
                var trueType = font.FontType == FontType.TrueType;
                sb.Append("data:font/")
                    .Append(trueType ? "ttf" : "otf")
                    .Append(";base64,")
                    .Append(Convert.ToBase64String(font.Buffer));
                sb.Append(@") 
            format('").Append(trueType ? "truetype" : "opentype").Append("\"')");
            }
            else
            {
                var path = sb.Prefix + $"font-{index + 1}.{(font.FontType == FontType.TrueType ? "ttf" : "otf")}";
                File.WriteAllBytes(Path.Combine(sb.Location, path), font.Buffer);
                sb.Append('\'').Append(path).Append("')");
            }

            sb.Append(";");
            if(font.HasUnicodeDictionary)
                sb.Append(@"
            dictionary-unicode:true;");
            sb.Append(@"
        }");
            sb.CreatedFont.Add(font.Name);
            return true;
        }


        public void AddFontClass(PdfTextResult text, PdfHtmlWriterContext sb)
        {
            sb.Append($@" font").Append(sb.fontRef[text.Font] + 1);
            if (text.Font.Bold)
                sb.Append($@" bold");
            sb.Append(" font-size-")
                .Append((Math.Round(text.FontSize * 2) / 2)
                    .ToString(formatNumInClassName));
        }
    }
}