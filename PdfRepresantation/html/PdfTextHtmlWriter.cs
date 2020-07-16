using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;

namespace PdfRepresantation
{
    public class PdfTextHtmlWriter
    {
        NumberFormatInfo formatNumInClassName = new NumberFormatInfo {NumberDecimalSeparator = "-"};
        protected readonly HtmlWriterConfig config;
        protected readonly PdfFontHtmlWriter fontWriter;

        public PdfTextHtmlWriter(HtmlWriterConfig config, PdfFontHtmlWriter fontWriter)
        {
            this.config = config;
            this.fontWriter = fontWriter;
        }

        
        public virtual void AddScriptInit(PdfHtmlWriterContext sb)
        {
            sb.Append(@"
            function BuildDarken(){
                var articles=document.getElementsByClassName('article');
                for (var i = 0; i < articles.length; i++) {
                     var article=articles[i];
                     var articleRect = article.getBoundingClientRect();
                     function addDarken(span) {
                         var rect = span.getBoundingClientRect();
                         var dark = document.createElement('dark');
                         dark.setAttribute('style', 'width:'+(rect.width|0)+'px;height:'+
                             (rect.height|0)+'px;top:'+((rect.top - articleRect.top-2)| 0)+'px;left:'
                             +((rect.left - articleRect.left-2)| 0)+'px' );
                         article.appendChild(dark);
                     }
                     var spans=article.getElementsByClassName('darken');
                     for (var j = 0; j < spans.length; j++) {
                         addDarken(spans[j]);
                     }
                }
            }
            BuildDarken();");
        }

        public virtual void AddLine(PdfPageDetails page,
            PdfTextLineDetails line, PdfHtmlWriterContext sb)
        {
            sb.Append($@"
        <div class=""line"" ");
            if (line.Blocks!=null)
                sb.Append("block-group=\"").Append(string.Join(",",line.Blocks)).Append("\" ");
            sb.Append("style=\"");
            if (line.Rotation.HasValue)
                sb.Append("transform: rotate(").Append(Math.Round(line.Rotation.Value)).Append("deg);");
            sb.Append("right:").Append(Math.Round(line.Right, config.RoundDigits))
                .Append("px;left:").Append(Math.Round(line.Left, config.RoundDigits))
                .Append("px;top:").Append(Math.Round(line.Top, config.RoundDigits))
                .Append("px;width:").Append(Math.Round(line.Width, config.RoundDigits))
                .Append("px;height:").Append(Math.Round(line.Height, config.RoundDigits))
                .Append("px;bottom:").Append(Math.Round(page.Height - line.Bottom, config.RoundDigits))
                .Append("px\" >");
            PdfLinkResult link = null;
            foreach (var text in line.Texts)
            {
                if (text.LinkParent != null)
                {
                    if (text.LinkParent != link)
                        AddLink(link = text.LinkParent, sb);
                    continue;
                }

                AddText(text, sb);
            }

            sb.Append(@"</div>");
        }

        protected virtual void AddText(PdfTextResult text, PdfHtmlWriterContext sb)
        {
            sb.Append($@"<span class=""baseline");
            fontWriter.AddFontClass(text, sb);
            var b = text.Stroke.MainColor?.GetBrightness();
            if (b > 0.9)
            {
                sb.Append($@" darken");
            }

            sb.Append("\" style=\"");
            AddColor(text, sb);
            sb.Append(@""">");
            AddText(text.Value, sb);
            sb.Append(@"</span>");
        }

        protected virtual void AddLink(PdfLinkResult link,  PdfHtmlWriterContext sb)
        {
            sb.Append($@"<a href=""").Append(link.Link).Append("\">");
            foreach (var text in link.Children)
            {
                AddText(text, sb);
            }

            sb.Append(@"</a>");
        }

       

        protected virtual void AddText(string text, PdfHtmlWriterContext sb)
        {
            var textEncoded = HttpUtility.HtmlEncode(text);
            bool lastSpace = false;
            foreach (var c in textEncoded)
            {
                if (c == ' ')
                {
                    if (lastSpace)
                        sb.Append("&nbsp;");
                    else
                        sb.Append(c);
                    lastSpace = true;
                    continue;
                }

                lastSpace = false;
                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
                else if (c <= 127)
                    sb.Append(c);
                else
                    sb.Append("&#").Append((int) c).Append(";");
            }
        }

        protected virtual void AddColor(PdfTextResult text, PdfHtmlWriterContext sb)
        {
            if (!text.Stroke.MainColor.HasValue)
                return;
            sb.Append("color:");
            PdfHtmlWriter.AppendColor(text.Stroke.MainColor.Value, sb);
            sb.Append(";");
        }

        public virtual void AddTextStyle(PdfHtmlWriterContext sb)
        {
            sb.Append(@"
        .line{
            position:absolute;
            min-width:fit-content;
            white-space: nowrap;
            transform-origin: bottom left;
        }
        .baseline{vertical-align:baseline;}
         dark{
            background-color: lightgray;
            display: block;
            position: absolute;
            z-index: -10;
         }");
        }
    }
}