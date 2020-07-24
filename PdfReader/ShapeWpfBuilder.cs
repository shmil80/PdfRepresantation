using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Shapes;
using PdfRepresantation;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;

namespace PdfReader
{
    class ShapeWpfBuilder
    {
        public void AddShape(ShapeDetails shape, PageContext pageContext)
        {
            var path = new Path();
            var pathGeometry = new PathGeometry();
            bool stroke,fill;
            if (shape.StrokeOperation)
            {
                path.Stroke = ConvertColor(shape.StrokeColor);
                stroke = true;
            }
            else
            {
                
                path.Stroke = Brushes.Transparent;
                stroke = false;
            }

            if (shape.FillOperation)
            {
                fill = true;
                path.Fill = ConvertColor(shape.FillColor);
                pathGeometry.FillRule = shape.EvenOddRule ? FillRule.EvenOdd : FillRule.Nonzero;
            }
            else
            {
                fill = false;
                path.Fill = Brushes.Transparent;
            }

            PathFigure figure = null;
            ShapePoint position = null;
            foreach (var line in shape.Lines)
            {
                if (!line.Start.Equals(position))
                {
                    figure = new PathFigure {StartPoint = ConvertPoint(line.Start)};
                    figure.IsFilled = fill;
                    pathGeometry.Figures.Add(figure);
                }
                
                figure.Segments.Add(ConvertLine(line, stroke));

                position = line.End;
            }

            path.Data = pathGeometry;
            pageContext.pagePanel.Children.Add(path);
        }

        private PathSegment ConvertLine(ShapeLine line, bool stroke)
        {
            var end = ConvertPoint(line.End);
            if (line.CurveControlPoint1 == null)
                return new LineSegment(end, stroke);
            var control1 = ConvertPoint(line.CurveControlPoint1);
            if (line.CurveControlPoint2 == null)
                return new QuadraticBezierSegment(control1, end, stroke);
            var control2 = ConvertPoint(line.CurveControlPoint2);
            return new BezierSegment(control1, control2, end, stroke);
        }

        private Brush ConvertColor(ColorDetails colorDetails)
        {
            switch (colorDetails)
            {
                case GardientColorDetails gardientDetails:
                    var gradients = new GradientStopCollection();
                    for (var i = 0; i < gardientDetails.Colors.Count; i++)
                    {
                        var c = gardientDetails.Colors[i];
                        var offset = c.OffSet ?? i / (double) gardientDetails.Colors.Count;
                        gradients.Add(new GradientStop(PdfWpfBuilder.ConvertColor(c.Color), offset));
                    }

                    return new LinearGradientBrush(gradients,
                        ConvertPoint(gardientDetails.Start),
                        ConvertPoint(gardientDetails.End));
                case SimpleColorDetails simpleDetails:
                    return new SolidColorBrush(PdfWpfBuilder.ConvertColor(simpleDetails.Color));
                default: throw new ArgumentOutOfRangeException(nameof(colorDetails));
            }
        }

        private Point ConvertPoint(GardientPoint gardientPoint)
        {
            return new Point(gardientPoint.RelativeX, gardientPoint.RelativeX);
        }


        private Point ConvertPoint(ShapePoint shapePoint)
        {
            return new Point(shapePoint.X, shapePoint.Y);
        }
    }
}