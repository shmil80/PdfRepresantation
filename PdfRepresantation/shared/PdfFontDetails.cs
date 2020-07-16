namespace PdfRepresantation
{
    public enum FontType
    {
        Unknown=0,
        Type1,
        TrueType,
        CFF_Type1,
        CFF_Type0,
        CFF_OpenType
    }

    public class PdfFontFileDetails
    {
        public byte[] Buffer { get; set; }
        public FontType FontType { get; set; }
        public string Name { get; set; }
    }
    public class PdfFontDetails
    {
        public string FontFamily { get; set; }
        public string BasicFontFamily { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public PdfFontFileDetails FontFile { get; set; }

        protected bool Equals(PdfFontDetails other) => string.Equals(FontFamily, other.FontFamily) &&
                                                       string.Equals(BasicFontFamily, other.BasicFontFamily) &&
                                                       Bold == other.Bold && Italic == other.Italic;

        public override bool Equals(object obj) => !ReferenceEquals(null, obj) &&
                                                   (ReferenceEquals(this, obj) ||
                                                    obj is PdfFontDetails details && Equals(details));

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FontFamily.GetHashCode();
                hashCode = (hashCode * 397) ^ (BasicFontFamily?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Bold.GetHashCode();
                hashCode = (hashCode * 397) ^ Italic.GetHashCode();
                return hashCode;
            }
        }
    }
}