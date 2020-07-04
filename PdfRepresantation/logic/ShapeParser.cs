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
        private readonly PageContext pageContext;

        internal ShapeParser(PageContext pageContext)
        {
            this.pageContext = pageContext;
        }

        public virtual void ParsePath(PathRenderInfo data, int orderIndex)
        {
            var shapeOperation = (ShapeOperation) data.GetOperation();
            bool evenOddRule = data.GetRule() == PdfCanvasConstants.FillingRule.EVEN_ODD;
            var lineWidth = data.GetLineWidth();
            var shapeDetails = new BaseShapeDetails
            {
                ShapeOperation = shapeOperation,
                LineWidth = lineWidth,
                EvenOddRule = evenOddRule,
            };
            if (shapeOperation == ShapeOperation.None)
                GetClippingPath(data,shapeDetails);
            else
                AddShapeToDraw(data, shapeDetails,orderIndex);
        }

        private void GetClippingPath(PathRenderInfo data, BaseShapeDetails baseShape)
        {
            //TODO not working yet. need adjusment on position and size.
            return;
            var lines = ConvertLines(data.GetPath(), null).ToArray();
            if (lines.Length == 0)
                return;
            var ctm = data.GetCtm();
            var scaleWidth = ctm.Get(Matrix.I11);
            var scaleHeight = ctm.Get(Matrix.I22);
            var x = ctm.Get(Matrix.I31);
            var y = ctm.Get(Matrix.I32);
            ClippingPath path = new ClippingPath
            {
                EvenOddRule = baseShape.EvenOddRule,
                Lines = lines,
                LineWidth = baseShape.LineWidth,
                ShapeOperation = baseShape.ShapeOperation,
                ScaleHeight = scaleHeight,
                ScaleWidth = scaleWidth,
                X = x,
                Y = y
            };
            pageContext.CurrentBBox = path;
        }


        private void AddShapeToDraw(PathRenderInfo data, BaseShapeDetails baseShape,int orderIndex)
        {
            var lines = ConvertLines(data.GetPath(), data.GetCtm()).ToArray();
            if (lines.Length == 0)
                return;
            var fillColor = ColorManager.GetColor(pageContext.Page, data.GetFillColor(),
                data.GetGraphicsState().GetFillOpacity());
            if (baseShape.ShapeOperation != ShapeOperation.Stroke && fillColor == null)
                return;
            var shape = new ShapeDetails
            {
                Order = orderIndex,
                EvenOddRule = baseShape.EvenOddRule,
                Lines = lines,
                LineWidth = baseShape.LineWidth,
                ShapeOperation = baseShape.ShapeOperation,
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

        protected ShapePoint ConvertPoint(Point p, Matrix ctm)
        {
            if (ctm == null)
                return new ShapePoint
                {
                    X = (float)p.x,
                    Y =  (float)p.y
                };
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