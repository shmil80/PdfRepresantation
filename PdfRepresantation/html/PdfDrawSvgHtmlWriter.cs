using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using iText.StyledXmlParser.Jsoup.Select;

namespace PdfRepresantation
{
    public class PdfDrawSvgHtmlWriter : PdfDrawHtmlWriter
    {
        public PdfDrawSvgHtmlWriter(bool embeddedImages, string dirImages) : base(embeddedImages, dirImages)
        {
        }

        public override void DrawShapesAndImages(PdfPageDetails page, StringBuilder sb)
        {
            sb.Append(@"
    <svg class=""canvas"" height=""").Append(Math.Round(page.Height, 2))
                .Append("\" width=\"").Append(Math.Round(page.Width, 2))
                .Append("\">");
            AddShapesAndImages(page, sb);

            sb.Append(@"
    </svg>");
        }


        protected override void AddImage(PdfImageDetails image, StringBuilder sb)
        {
            sb.Append(@"
        <image height=""").Append(Math.Round(image.Height, 2))
                .Append("\" width=\"")
                .Append(Math.Round(image.Width, 2)).Append("\" href=\"");
            this.AssignPathImage(image, sb);
            sb.Append("\" x=\"").Append(Math.Round(image.Left, 2))
                .Append("\" y=\"").Append(Math.Round(image.Top, 2)).Append("\"/>");
            ;
        }

        private void AddPoint(ShapePoint p, StringBuilder sb)
        {
            sb.Append(" ").Append(Math.Round(p.X, 2)).Append(" ").Append(Math.Round(p.Y, 2));
        }

        protected override void InitGradients(Dictionary<GardientColorDetails, int> gradients, StringBuilder sb)
        {
            sb.Append(@"        
        <defs>");
            foreach (var pair in gradients)
            {
                var i = pair.Value;
                var g = pair.Key;
                if (g.ColorStart == null||g.ColorEnd==null)
                    continue;
                sb.Append(@"
            <linearGradient id=""gradient-").Append(i);
                if (g.ColorStart != null)
                    sb.Append(@""" x1=""").Append(g.ColorStart.RelativeX.ToString("P2"))
                        .Append(@""" x2=""").Append(g.ColorEnd.RelativeX.ToString("P2"))
                        .Append(@""" y1=""").Append(g.ColorStart.RelativeY.ToString("P2"))
                        .Append(@""" y2=""").Append(g.ColorEnd.RelativeY.ToString("P2"));
                sb.Append(@""">
                <stop offset=""0%"" stop-color=""");
                if (g.ColorStart != null)
                    PdfHtmlWriter.AppendColor(g.ColorStart.Color, sb);
                sb.Append(@"""/>
                <stop offset=""100%"" stop-color=""");
                PdfHtmlWriter.AppendColor(g.ColorEnd.Color, sb);
                sb.Append(@"""/>
            </linearGradient>");
            }

            sb.Append(@"
        </defs>").Append("");
        }

        protected override void AddShape(ShapeDetails shape, StringBuilder sb,
            Dictionary<GardientColorDetails, int> gradients)
        {
            if (shape.ShapeOperation == ShapeOperation.None)
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
            AppendColor(shape.ShapeOperation == ShapeOperation.Stroke ? null : shape.FillColor, gradients, sb);
            if (shape.EvenOddRule)
                sb.Append("\" fill-rule=\"evenodd");
            sb.Append("\" stroke=\"");
            AppendColor(shape.ShapeOperation == ShapeOperation.Fill ? null : shape.StrokeColor, gradients, sb);
            sb.Append("\"/>");
        }

        private void AppendColor(ColorDetails color, Dictionary<GardientColorDetails, int> gradients, StringBuilder sb)
        {
            switch (color)
            {
                case SimpleColorDetails simpleColor:
                    PdfHtmlWriter.AppendColor(simpleColor.Color, sb);
                    break;
                case GardientColorDetails g:
                    sb.Append("url(#gradient-").Append(gradients[g]).Append(")");
                    break;
                default:
                    sb.Append("transparent");
                    break;
            }
        }

        public override void AddScript(StringBuilder sb)
        {
        }
    }
}