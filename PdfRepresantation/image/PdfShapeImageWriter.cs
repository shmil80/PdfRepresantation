﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PdfRepresantation
{
    public class PdfShapeImageWriter
    {
        public virtual void DrawShape(Graphics graphics, PdfPageDetails page, ShapeDetails shape, float top)
        {
            GraphicsPath path = new GraphicsPath();
            CreateLinesInPath(path, shape.Lines,top);
            switch (shape.ShapeOperation)
            {
                case ShapeOperation.Stroke:
                    StrokeShape(graphics, path, shape);
                    break;
                case ShapeOperation.Fill:
                    FillShape(graphics, path, shape);
                    break;
                case ShapeOperation.Both:
                    StrokeShape(graphics, path, shape);
                    FillShape(graphics, path, shape);
                    break;
            }
        }

        protected virtual void CreateLinesInPath(GraphicsPath path, IList<ShapeLine> lines,float top)
        {
            foreach (var line in lines)
            {
                var start = new PointF(line.Start.X,top+ line.Start.Y);
                var end = new PointF(line.End.X, top + line.End.Y);
                if (line.CurveControlPoint1 == null)
                {
                    path.AddLine(start, end);
                    continue;
                }
                var controlPoint1 = new PointF(line.CurveControlPoint1.X, top + line.CurveControlPoint1.Y);
                if (line.CurveControlPoint2 == null)
                {
                    path.AddBeziers(new PointF[] { start, controlPoint1, end });
                    continue;
                }
                var controlPoint2 = new PointF(line.CurveControlPoint2.X, top + line.CurveControlPoint2.Y);
                path.AddBezier(start, controlPoint1, controlPoint2, end);


            }
        }

        protected virtual void StrokeShape(Graphics graphics, GraphicsPath path, ShapeDetails shape)
        {
            Brush brush = CreateBrush(shape.StrokeColor);
            Pen pen = new Pen(brush, shape.LineWidth);
            graphics.DrawPath(pen, path);
        }

        protected virtual void FillShape(Graphics graphics, GraphicsPath path, ShapeDetails shape)
        {
            Brush brush = CreateBrush(shape.FillColor);
            if (shape.EvenOddRule)
                path.FillMode = FillMode.Alternate;
            else
                path.FillMode = FillMode.Winding;
            path.CloseFigure();
            graphics.FillPath(brush, path);
        }

        protected virtual Brush CreateBrush(ColorDetails color)
        {
            Brush brush;
            switch (color)
            {
                case null: return Brushes.Transparent;
                case SimpleColorDetails simpleColor:
                    brush = new SolidBrush(simpleColor.Color);
                    break;
                case GardientColorDetails gardientColor:
                    //TODO
                    brush=Brushes.Blue;                    
//                    brush = new LinearGradientBrush();
                    break;
                default: throw new IndexOutOfRangeException();
            }

            return brush;
        }
    }
}