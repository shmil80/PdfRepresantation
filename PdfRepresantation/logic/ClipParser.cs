using System;
using System.Linq;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser.Data;

namespace PdfRepresantation
{
    public class ClipParser : PathParser
    {
        internal ClipParser(PageContext pageContext) : base(pageContext)
        {
        }

        public virtual void ParseClip(PathRenderInfo data)
        {
            //TODO not working yet. need adjusment on position and size.
            return;
            var lines = ConvertLines(data.GetPath(), data.GetCtm(),200).ToArray();
            if (lines.Length == 0)
                return;

            var ctm = data.GetCtm();
            var scaleWidth = ctm.Get(Matrix.I11);
            var scaleHeight = ctm.Get(Matrix.I22);
            var x = ctm.Get(Matrix.I31);
            var y = ctm.Get(Matrix.I32);
            bool evenOddRule = data.GetRule() == PdfCanvasConstants.FillingRule.EVEN_ODD;
            ClippingPath path = new ClippingPath
            {
                Lines = lines,
                EvenOddRule = evenOddRule,
                ScaleHeight = scaleHeight,
                ScaleWidth = scaleWidth,
                X = x,
                Y = y
            };
            pageContext.CurrentBBox = path;
        }


        protected override ShapePoint ConvertPoint(Point p, Matrix ctm)
        {
            return new ShapePoint
            {
                X = (float) p.x,
                Y = (float) p.y
            }; 
            var scaleWidth = ctm.Get(Matrix.I11);
            var scaleHeight = ctm.Get(Matrix.I22);
            return new ShapePoint
            {
                X = (float) p.x/Math.Abs(scaleWidth),
                Y = (float) p.y/Math.Abs(scaleHeight)
            };
        }
    }
}