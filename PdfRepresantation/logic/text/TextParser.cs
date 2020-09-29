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


            var position = new RectangleRotated(baseline,ascentLine);
            PdfTextBlock item = new PdfTextBlock
            {
                Value = text,
                Bottom = position.Bottom,
                Left = position.Left,
                Width = position.Width,
                Height = position.Height,
                Rotation = position.Angle,
                StrokeColore = TextRenderManager.GetColor(pageContext.Page, textRenderInfo),
                SpaceWidth = textRenderInfo.GetSingleSpaceWidth(),
                Font = pageContext.FontManager.GetFont(textRenderInfo.GetFont()),
            };
            if (string.IsNullOrWhiteSpace(text) && texts.Count > 0)
                item.Group = texts[texts.Count - 1].Group;
            item.FontSize = pageContext.FontManager.GetFontSize(textRenderInfo, item);
            RightToLeftManager.Instance.AssignRtl(item, textRenderInfo.GetUnscaledWidth() < 0);
            pageContext.LinkManager.AssignLink(item);
            texts.Add(item);
        }

        

        private int blockIndex;

        public void MarkAsEnd()
        {
            if (texts.Count == 0 || texts[texts.Count - 1].Group.HasValue)
                return;
            blockIndex++;
            for (var i = texts.Count - 1; i >= 0; i--)
            {
                if (texts[i].Group.HasValue)
                    break;
                texts[i].Group = blockIndex;
            }
        }
    }
}