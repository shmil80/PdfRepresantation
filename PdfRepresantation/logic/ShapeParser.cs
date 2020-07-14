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
    public class ShapeParser 
    {
        public readonly IList<ShapeDetails> shapes = new List<ShapeDetails>();
        protected readonly PageContext pageContext;

        internal ShapeParser(PageContext pageContext)
        {
            this.pageContext = pageContext;
        }

        public virtual void ParsePath(PathRenderInfo data, int orderIndex)
        {
            var shapeOperation = (ShapeOperation) data.GetOperation();
            bool evenOddRule = data.GetRule() == PdfCanvasConstants.FillingRule.EVEN_ODD;
            var lines = ConvertLines(data.GetPath(), data.GetCtm()).ToArray();
            if (lines.Length == 0)
                return;
            ClippingPath clip;
            if (shapeOperation == ShapeOperation.None)
            {
                clip = CreateClip(evenOddRule, lines);
            }
            else
            {
                var shape = CreateShape(data, orderIndex, evenOddRule, shapeOperation, lines);
                shapes.Add(shape);
                clip = shape;
            }
            if(data.IsPathModifiesClippingPath())
                pageContext.Processor.Clip(clip);
        }

        private ClippingPath CreateClip(bool evenOddRule, ShapeLine[] lines)
        {
            return new ClippingPath
            {
                Lines = lines,
                EvenOddRule = evenOddRule,
            };
        }

        private ShapeDetails CreateShape(PathRenderInfo data, int orderIndex, bool evenOddRule,
            ShapeOperation shapeOperation, ShapeLine[] lines)
        {
            var fillColor = ColorManager.GetColor(pageContext.Page, data.GetFillColor(),
                data.GetGraphicsState().GetFillOpacity());
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
            var dash = data.GetLineDashPattern();
            shape.StrokeColor = strokeColor;
            shape.FillColor = fillColor;
            if (Log.DebugSupported)
            {
                Log.Debug($"shape: {shape}");
            }

            return shape;
        }
        protected IEnumerable<ShapeLine> ConvertLines(Path path, Matrix ctm)
        {
            return from subpath in path.GetSubpaths()
                from line in subpath.GetSegments()
                select ConvertLine(line, ctm);
        }

        protected ShapeLine ConvertLine(IShape line, Matrix ctm)
        {
            var points = line.GetBasePoints()
                .Select(p => ConvertPoint(p, ctm))
                .ToArray();
            var result = new ShapeLine
            {
                Start = points[0],
                End = points[points.Length - 1]
            };
            if (points.Length > 2)
            {
                result.CurveControlPoint1 = points[1];
                if (points.Length > 3)
                    result.CurveControlPoint2 = points[2];
            }

            return result;
        }
        protected virtual ShapePoint ConvertPoint(Point p, Matrix ctm)
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