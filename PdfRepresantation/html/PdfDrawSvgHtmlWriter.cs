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
        public PdfDrawSvgHtmlWriter(HtmlWriterConfig config) : base(config)
        {
        }

        protected override PdfImageHtmlWriter CreateImageWriter()
            => new PdfImageHtmlSvgWriter(config);

        public override void DrawShapesAndImages(PdfPageDetails page, PdfHtmlWriterContext sb)
        {
            sb.Append(@"
    <svg class=""canvas"" height=""").Append(Math.Round(page.Height, config.RoundDigits))
                .Append("\" width=\"").Append(Math.Round(page.Width, config.RoundDigits))
                .Append("\">");
            AddShapesAndImages(page, sb);

            sb.Append(@"
    </svg>");
        }


        private void AddPoint(ShapePoint p, PdfHtmlWriterContext sb)
        {
            sb.Append(" ").Append(Math.Round(p.X, config.RoundDigits)).Append(" ")
                .Append(Math.Round(p.Y, config.RoundDigits));
        }

        protected override void InitGradients(Dictionary<GardientColorDetails, int> gradients, PdfHtmlWriterContext sb)
        {
            sb.Append(@"        
        <defs>");
            foreach (var pair in gradients)
            {
                var i = pair.Value;
                var g = pair.Key;
                if (g.Colors.Count == 0)
                    continue;
                sb.Append(@"
            <linearGradient id=""gradient-").Append(i);
                if (g.Start != null)
                    sb.Append(@""" x1=""").Append(g.Start.RelativeX.ToString("P" + config.RoundDigits))
                        .Append(@""" x2=""").Append(g.End.RelativeX.ToString("P" + config.RoundDigits))
                        .Append(@""" y1=""").Append(g.Start.RelativeY.ToString("P" + config.RoundDigits))
                        .Append(@""" y2=""").Append(g.End.RelativeY.ToString("P" + config.RoundDigits));
                sb.Append(@""">");
                foreach (var color in g.Colors)
                {
                    sb.Append(@"
                <stop ");
                    if (color.OffSet.HasValue)
                        sb.Append(@"offset=""").Append(color.OffSet.Value.ToString("P" + config.RoundDigits))
                            .Append(@"""");
                    sb.Append(@" stop-color=""");
                    PdfHtmlWriter.AppendColor(color.Color, sb);
                    sb.Append(@"""/>");
                }

                sb.Append(@"
            </linearGradient>");
            }

            sb.Append(@"
        </defs>").Append("");
        }

        protected override void AddShape(ShapeDetails shape, PdfHtmlWriterContext sb,
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
            AppendColor(!shape.FillOperation ? null : shape.FillColor, gradients, sb);
            if (shape.EvenOddRule)
                sb.Append("\" fill-rule=\"evenodd");
            sb.Append("\" stroke=\"");
            AppendColor(!shape.StrokeOperation ? null : shape.StrokeColor, gradients, sb);
            sb.Append("\"/>");
        }

        private void AppendColor(ColorDetails color, Dictionary<GardientColorDetails, int> gradients,
            PdfHtmlWriterContext sb)
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

        public override void AddScript(PdfHtmlWriterContext sb)
        {
        }
    }
}