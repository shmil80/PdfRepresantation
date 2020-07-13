using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using Point = iText.Kernel.Geom.Point;

namespace PdfRepresantation
{
    public class ShapeParser : PathParser
    {
        public readonly IList<ShapeDetails> shapes = new List<ShapeDetails>();

        internal ShapeParser(PageContext pageContext) : base(pageContext)
        {
        }

        public virtual void ParsePath(PathRenderInfo data, int orderIndex)
        {
            var shapeOperation = (ShapeOperation) data.GetOperation();
            if (shapeOperation == ShapeOperation.None)
                return;
            var lines = ConvertLines(data.GetPath(), data.GetCtm()).ToArray();
            if (lines.Length == 0)
                return;
            var fillColor = ColorManager.GetColor(pageContext.Page, data.GetFillColor(),
                data.GetGraphicsState().GetFillOpacity());
            if (shapeOperation != ShapeOperation.Stroke && fillColor == null)
                return;
            bool evenOddRule = data.GetRule() == PdfCanvasConstants.FillingRule.EVEN_ODD;
            var lineWidth = data.GetLineWidth();
            var shape = new ShapeDetails
            {
                Order = orderIndex,
                EvenOddRule = evenOddRule,
                Lines = lines,
                LineWidth = lineWidth,
                ShapeOperation = shapeOperation,
            };
            if (fillColor is GardientColorDetails gradient)
            {
                var minX = shape.MinX;
                var minY = shape.MinY;
                var maxX = shape.MaxX;
                var maxY = shape.MaxY;
                ColorManagerPattern.CalculteRelativePosition(gradient,
                    minX, minY, maxX, maxY);
            }

            var strokeColor = ColorManager.GetColor(pageContext.Page, data.GetStrokeColor(),
                data.GetGraphicsState().GetStrokeOpacity());
            var lineCap = data.GetLineCapStyle();
            shape.StrokeColor = strokeColor;
            shape.FillColor = fillColor;
            if (Log.DebugSupported)
            {
                Log.Debug($"shape: {shape}");
            }

            shapes.Add(shape);
        }

        protected override ShapePoint ConvertPoint(Point p, Matrix ctm)
        {
            Vector vector = new Vector((float) p.x, (float) p.y, 1);
            vector = vector.Cross(ctm);
            return new ShapePoint
            {
                X = vector.Get(Vector.I1),
                Y = pageContext.PageHeight - vector.Get(Vector.I2)
            };
        }
    }
}