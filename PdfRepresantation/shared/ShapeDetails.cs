﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PdfRepresantation
{
    public enum ShapeOperation
    {
        None = 0,
        Stroke = 1,
        Fill = 2,
        Both = 3
    }

    public class ClippingPath
    {
        public bool EvenOddRule { get; set; }
        public IList<ShapeLine> Lines = new List<ShapeLine>();
        public float MinX => Lines.Min(l => l.AllPoints.Min(p => p.X));
        public float MinY => Lines.Min(l => l.AllPoints.Min(p => p.Y));
        public float MaxX => Lines.Max(l => l.AllPoints.Max(p => p.X));
        public float MaxY => Lines.Max(l => l.AllPoints.Max(p => p.Y));
        public ClippingPath Clone()
        {
            return new ClippingPath
            {
                EvenOddRule = EvenOddRule,
                Lines = new List<ShapeLine>(Lines)
            };
        } }

    public class ShapeDetails : ClippingPath, IPdfDrawingOrdered
    {
        public ShapeOperation ShapeOperation { get; set; }
        public float LineWidth { get; set; }
        public int Order { get; set; }
        public ColorDetails StrokeColor { get; set; }
        public ColorDetails FillColor { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("").Append("")
                .Append("start:")
                .Append(MinX.ToString("F2")).Append(",")
                .Append(MinY.ToString("F2"))
                .Append(" end:")
                .Append(MaxX.ToString("F2")).Append(",")
                .Append(MaxY.ToString("F2"));
            switch (ShapeOperation)
            {
                case ShapeOperation.None: break;
                case ShapeOperation.Stroke:
                    sb.Append(" stroke:").Append(StringifyColor(StrokeColor));
                    break;
                case ShapeOperation.Fill:
                    sb.Append(" fill:").Append(StringifyColor(FillColor));
                    break;
                case ShapeOperation.Both:
                    sb.Append(" stroke:").Append(StringifyColor(StrokeColor));
                    sb.Append(" fill:").Append(StringifyColor(FillColor));
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            return sb.ToString();
        }

        string StringifyColor(ColorDetails c)
        {
            if (!(c is SimpleColorDetails simpleColor))
                return "-";
            var color = simpleColor.Color;
            if (color.A == 0)
                return "transparent";
            if (color.R == 0)
            {
                if (color.G == 0)
                {
                    if (color.B == 0) return "black";
                    if (color.B == 255) return "blue";
                }
                else if (color.G == 255)
                {
                    if (color.B == 0) return "green";
                    if (color.B == 255) return "ciel";
                }
            }
            else if (color.R == 255)
            {
                if (color.G == 0)
                {
                    if (color.B == 0) return "red";
                    if (color.B == 255) return "purple";
                }
                else if (color.G == 255)
                {
                    if (color.B == 0) return "yellow";
                    if (color.B == 255) return "white";
                }
            }

            if (color.R == color.G && color.R == color.B)
                return $"gray ({color.R})";
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}{(color.A == 255 ? "" : color.A.ToString("X2"))}";
        }
    }

}