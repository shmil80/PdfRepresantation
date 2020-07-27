//MIT, 2016-present, WinterDev 
 
using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Typography.OpenFont.Tables; 

namespace Typography.FontCollections
{

    public class InstalledTypeface
    {
        readonly NameEntry _nameEntry;
        readonly OS2Table _os2Table;
        internal InstalledTypeface(Typeface typeface, IFontStreamSource fontPath)
            : this(typeface.GetNameEntry(),
                  typeface.GetOS2Table(),
                  typeface.Languages,
                  fontPath)
        { }

        public InstalledTypeface(PreviewFontInfo previewFontInfo, IFontStreamSource fontPath)
            : this(previewFontInfo.NameEntry,
                  previewFontInfo.OS2Table,
                  previewFontInfo.Languages, fontPath)
        { }

        private InstalledTypeface(NameEntry nameTable, OS2Table os2Table, Languages languages, IFontStreamSource fontPath)
        {
            _nameEntry = nameTable;
            _os2Table = os2Table;
            FontPath = fontPath;
            Languages = languages;

            if (os2Table != null)
            {
                var fsSelection = new OS2FsSelection(os2Table.fsSelection);
                TypefaceStyle = fsSelection.IsItalic ? TypefaceStyle.Italic : TypefaceStyle.Regular;
            }
        }
        public TypefaceStyle TypefaceStyle { get; internal set; }

        public string FontName => _nameEntry.FontName;
        public string FontSubFamily => _nameEntry.FontSubFamily;

        public string TypographicFamilyName => _nameEntry.TypographicFamilyName;
        public string TypographicFontSubFamily => _nameEntry.TypographyicSubfamilyName;
        public string PostScriptName => _nameEntry.PostScriptName;
        public string UniqueFontIden => _nameEntry.UniqueFontIden;
        public ushort WeightClass => _os2Table?.usWeightClass??0;
        public ushort WidthClass => _os2Table?.usWidthClass??0;

        public Languages Languages { get; }
        public IFontStreamSource FontPath { get; internal set; }
        public int ActualStreamOffset { get;  set; }

        //TODO: UnicodeLangBits vs UnicodeLangBits5_1
        public bool DoesSupportUnicode(BitposAndAssciatedUnicodeRanges bitposAndAssocUnicode) => OpenFontUnicodeUtilExtensions.DoesSupportUnicode(Languages, bitposAndAssocUnicode.Bitpos);
        public bool DoesSupportUnicode(int bitpos) => OpenFontUnicodeUtilExtensions.DoesSupportUnicode(Languages, bitpos);

        /// <summary>
        /// check if this font has glyph for the given code point or not
        /// </summary>
        /// <returns></returns>
        public bool ContainGlyphForUnicode(int codepoint) => Languages.ContainGlyphForUnicode(codepoint);

        internal Typeface ResolvedTypeface;

#if DEBUG
        public override string ToString()
        {
            return FontName + " (" + FontSubFamily + ")";
        }
#endif
    }

}