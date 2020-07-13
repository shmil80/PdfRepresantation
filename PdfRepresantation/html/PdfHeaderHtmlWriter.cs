using System;
using System.Text;

namespace PdfRepresantation
{
    public class PdfHeaderHtmlWriter
    {
        public virtual void AddHeader(PdfPageDetails page, StringBuilder sb)
        {
            sb.Append(@"
    <h2 class=""header"" style=""width: ").Append(Math.Round(page.Width, 2))
                .Append("px;--text:'").Append(HeaderText(page)).Append("'\"></h2>");
        }

        protected virtual string HeaderText(PdfPageDetails page)
        {
            return "Page "+page.PageNumber;
        }

        public virtual void AddStyle(StringBuilder sb)
        {
            sb.Append(@"
        .header{
            color: #795548;
            font-family: Arial;
            text-align: center;
            margin:20px auto 0 auto;
        }
        .header:before{content:var(--text);}");
        }
    }
}