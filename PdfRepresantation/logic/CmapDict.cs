using System.Collections.Generic;
using Typography;
using Typography.FontCollections;
using Typography.OpenFont;

namespace PdfRepresantation
{
    public class CmapDict
    {
        private List<Typeface> faces=new List<Typeface>();
  private Dictionary<ushort,char> codesToUnicode=new Dictionary<ushort,char>();

        public CmapDict(string path)
        {
            var fonts = new InstalledTypefaceCollection();
            fonts.AddFontStreamSource(new FontFileStreamProvider(path));
            faces = new List<Typeface>();
            foreach (var font in fonts.GetInstalledFontIter())
            {
                var typeFace = fonts.ResolveTypeface(font);
                if (typeFace != null)
                    faces.Add(typeFace);
            }

           
        }
        public CmapDict(byte[] buffer)
        {
            var fonts = new InstalledTypefaceCollection();
            fonts.AddFontStreamSource(new FontBufferStreamProvider(buffer));
            foreach (var font in fonts.GetInstalledFontIter())
            {
                var typeFace = fonts.ResolveTypeface(font);
                if (typeFace != null)
                faces.Add(typeFace);
            }

        }
        
        public int GetCode(char c)
        {
            foreach (var typeface in faces)
            {
                var cid = typeface.GetGlyphIndex(c);
                if (cid != 0)
                    return cid;
            }

            return 0;
        }
    }
}