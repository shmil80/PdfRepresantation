using System.Collections.Generic;
using System.Text;

namespace PdfRepresantation
{
    public class PdfHtmlWriterContext
    {
        private readonly StringBuilder sb = new StringBuilder();
        public Dictionary<PdfFontDetails, int> fontRef { get; set; }
        public string Text => sb.ToString();
        public readonly ISet<string> CreatedFont =new HashSet<string>();

        public readonly string Location;

        public readonly string Prefix;

        public PdfHtmlWriterContext(string prefix, string location)
        {
            this.Prefix = prefix;
            Location = location;
        }

        public PdfHtmlWriterContext Append(string s)
        {
            sb.Append(s);
            return this;
        }

        public PdfHtmlWriterContext Append(char s)
        {
            sb.Append(s);
            return this;
        }

        public PdfHtmlWriterContext Append(double s)
        {
            sb.Append(s);
            return this;
        }

        public PdfHtmlWriterContext Append(float? s)
        {
            sb.Append(s);
            return this;
        }

        public PdfHtmlWriterContext Append(int s)
        {
            sb.Append(s);
            return this;
        }

        public PdfHtmlWriterContext Append(float s)
        {
            sb.Append(s);
            return this;
        }
    }
}