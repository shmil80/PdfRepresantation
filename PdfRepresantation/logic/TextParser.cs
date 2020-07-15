using System.Collections.Generic;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace PdfRepresantation
{
    public class TextParser
    {
        public readonly IList<PdfTextBlock> texts = new List<PdfTextBlock>();
        public readonly Dictionary<PdfFont, PdfFontDetails> fonts = new Dictionary<PdfFont, PdfFontDetails>();
        private readonly PageContext pageContext;

        internal TextParser(PageContext pageContext)
        {
            this.pageContext = pageContext;
        }

 
        public virtual void ParseText(TextRenderInfo textRenderInfo)
        {
            var text = textRenderInfo.GetText();
            LineSegment baseline = textRenderInfo.GetBaseline();
            LineSegment ascentLine = textRenderInfo.GetAscentLine();
            if (textRenderInfo.GetRise() != 0)
            {
                Matrix m = new Matrix(0.0f, -textRenderInfo.GetRise());
                baseline = baseline.TransformBy(m);
                ascentLine = ascentLine.TransformBy(m);
            }


            var position = new RectangleRotated(baseline.GetStartPoint(),
                baseline.GetEndPoint(),
                ascentLine.GetStartPoint());
            PdfTextBlock item = new PdfTextBlock
            {
                Value = text,
                Bottom = pageContext.PageHeight - position.Bottom,
                Left = position.Left,
                Width = position.Width,
                Height = position.Height,
                Rotation = position.Angle,
                BottomInOwnPlane = pageContext.PageHeight -position.BottomInOwnPlane,
                StrokeColore = TextRenderManager.GetColor(pageContext.Page, textRenderInfo),
                SpaceWidth = textRenderInfo.GetSingleSpaceWidth(),
                Font = GetFont(textRenderInfo),
            };
            item.FontSize = FontManager.Instance.GetFontSize(textRenderInfo, item);
            RightToLeftManager.Instance.AssignRtl(item, textRenderInfo.GetUnscaledWidth() < 0);
            pageContext.LinkManager.AssignLink(item);
            texts.Add(item);
        }

        private PdfFontDetails GetFont(TextRenderInfo textRenderInfo)
        {
            var pdfFont = textRenderInfo.GetFont();
            if (!fonts.TryGetValue(pdfFont, out var font))
            {
                font = FontManager.Instance.CreateFont(pdfFont);
                fonts.Add(pdfFont, font);
            }

            return font;
        }

        public void MarkAsEnd()
        {
            if(texts.Count>0)
                texts[texts.Count - 1].EndBlock = true;
        }
    }
}