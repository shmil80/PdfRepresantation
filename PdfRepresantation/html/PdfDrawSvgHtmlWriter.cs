using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PdfRepresantation
{
    public class PdfDrawSvgHtmlWriter : PdfDrawHtmlWriter
    {

        public PdfDrawSvgHtmlWriter(bool embeddedImages, string dirImages) : base(embeddedImages, dirImages)
        {
        }
        public override void AddShapes(PdfPageDetails page, StringBuilder sb)
        {
            sb.Append(@"
    <svg class=""canvas"" height=""").Append(Math.Round(page.Height))
                .Append("\" width=\"").Append(Math.Round(page.Width))
                .Append("\">");
            AddShapesAndImages(page, sb);

            sb.Append(@"
    </svg>");
        }


        protected override void AddImage(PdfImageDetails image, StringBuilder sb)
        {
            sb.Append(@"
        <image height=""").Append(image.Height)
                .Append("\" width=\"")
                .Append(image.Width).Append("\" href=\"");
            this.AssignPathImage(image,sb);            
            sb.Append("\" x=\"").Append((int) (image.Left))
                .Append("\" y=\"").Append((int) (image.Top)).Append("\"/>");
            ;
        }

        private void AddPoint(ShapePoint p, StringBuilder sb)
        {
            sb.Append(" ").Append(Math.Round(p.X,2)).Append(" ").Append(Math.Round(p.Y,2));
        }

        protected override void AddShape(ShapeDetails shape, StringBuilder sb)
        {
            if(shape.ShapeOperation==ShapeOperation.None)
                return;
            ShapePoint lastEnd = null;
            sb.Append(@"
        <path d=""");
            foreach (var line in shape.Lines)
            {
                if (!line.Start.Equals(lastEnd))
                {
                    sb.Append("M");
                    AddPoint(line.Start, sb);
                }

                if (line.CurveControlPoint1 == null)
                    sb.Append(" L");
                else
                {
                    if (line.CurveControlPoint2 == null)
                    {
                        sb.Append(" Q");
                        AddPoint(line.CurveControlPoint1, sb);
                        sb.Append(",");
                    }
                    else
                    {
                        sb.Append(" C");
                        AddPoint(line.CurveControlPoint1, sb);
                        sb.Append(",");
                        AddPoint(line.CurveControlPoint2, sb);
                        sb.Append(",");
                    }
                }

                AddPoint(line.End, sb);
                lastEnd = line.End;
            }

            sb.Append("\" stroke-width=\"").Append(shape.LineWidth)
                .Append("\" fill=\"");
            AppendColor(shape.ShapeOperation==ShapeOperation.Stroke?null:shape.FillColor, sb);
           if(shape.EvenOddRule)
                sb.Append("\" fill-rule=\"evenodd");
            sb.Append("\" stroke=\"");
            AppendColor(shape.ShapeOperation==ShapeOperation.Fill?null:shape.StrokeColor, sb);
            sb.Append("\"/>");
        }

        private void AppendColor(ColorDetails color, StringBuilder sb)
        {
            if (color is SimpleColorDetails simpleColor)
            {
                PdfHtmlWriter.AppendColor(simpleColor.Color, sb);
            }
            else
            {
                sb.Append("transparent");
            }
        }

        public override void AddScript(StringBuilder sb)
        {
        }
    }
}