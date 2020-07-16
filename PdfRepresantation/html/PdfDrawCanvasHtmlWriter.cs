using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PdfRepresantation
{
    public class PdfDrawCanvasHtmlWriter : PdfDrawHtmlWriter
    {
 
        public override void DrawShapesAndImages(PdfPageDetails page, PdfHtmlWriterContext sb)
        {
            var width = Math.Round(page.Width, config.RoundDigits);
            var height = Math.Round(page.Height, config.RoundDigits);
            sb.Append(@"
    <canvas class=""canvas"" id=""canvas-").Append(page.PageNumber)
                .Append("\" style=\"width: ")
                .Append(width)
                .Append("px;height:").Append(height)
                .Append("px;\" width=\" ")
                .Append(width)
                .Append("\" height=\"").Append(height).Append("\"></canvas>");

            sb.Append(@"
    <script>
        currentCanvas= document.getElementById('canvas-").Append(page.PageNumber).Append(@"');");
            AddShapesAndImages(page, sb);

            sb.Append(@"
    </script>");
        }

        protected override PdfImageHtmlWriter CreateImageWriter()
            => new PdfImageHtmlCanvasWriter(config);

        protected override void InitGradients(Dictionary<GardientColorDetails, int> gradients, PdfHtmlWriterContext sb)
        {
        }

        protected override void AddShape(ShapeDetails shape, PdfHtmlWriterContext sb,
            Dictionary<GardientColorDetails, int> gradients)
        {
            sb.Append(@"
        draw([");
            for (var i = 0; i < shape.Lines.Count; i++)
            {
                if (i != 0)
                    sb.Append(",");
                sb.Append("[");
                var points = shape.Lines[i].AllPoints.ToArray();
                for (var j = 0; j < points.Length; j++)
                {
                    if (j != 0)
                        sb.Append(",");
                    var p = points[j];
                    sb.Append(p.X).Append(",").Append(p.Y);
                }

                sb.Append("]");
            }

            sb.Append("],").Append((int) shape.ShapeOperation).Append(",");
            AppendColor(shape.StrokeColor, sb);
            sb.Append(",");
            AppendColor(shape.FillColor, sb);
            sb.Append(",").Append(shape.LineWidth)
                .Append(",'").Append(shape.EvenOddRule ? "evenodd" : "nonzero").Append("',").Append("null")
                .Append(");");
        }

        public override void AddScript(PdfHtmlWriterContext sb)
        {
            sb.Append(@"
    <script>
        var currentCanvas;
        function draw(lines,operation,strokeColor, fillColor, lineWidth,fillRule,lineCap) {
            if (!currentCanvas.getContext)
                return;
            var ctx = currentCanvas.getContext('2d');
            if (lineWidth) 
                ctx.lineWidth = lineWidth;
            if (!lineCap) 
                ctx.lineCap= lineCap;
            if (!fillColor) 
                ctx.fillStyle = 'white';
            else if (fillColor.constructor !== Array) 
                ctx.fillStyle = fillColor;
            else {
                var grd = ctx.createLinearGradient(fillColor[0], fillColor[1], fillColor[2], fillColor[3]);
                for (var i = 4; i < fillColor.length; i += 2) {
                    grd.addColorStop(fillColor[i], fillColor[i + 1]);
                }
                ctx.fillStyle = grd;
            }
            ctx.strokeStyle=strokeColor||'black';
            ctx.beginPath();
            var position={x:'-',y:'-'};
            var drawLine=function (line) {
                if (position.x!=line[0]||position.y!=line[1])
                    ctx.moveTo(line[0], line[1]);
                switch (line.length)
                {
                    case 4:ctx.lineTo(line[2], line[3]);break;
                    case 6:ctx.quadraticCurveTo(line[2], line[3],line[4], line[5]);break;
                    case 8:ctx.bezierCurveTo(line[2], line[3],line[4], line[5],line[6], line[7]);break;
                }
                position.x=line[line.length-2];
                position.y=line[line.length-1];
            }; 
            for (var i = 0; i < lines.length; i++) 
                drawLine(lines[i]);
            switch (operation) {
                case 1:ctx.stroke();break;
                case 2:ctx.fill(fillRule);break;
                case 3:ctx.stroke();
                   ctx.fill(fillRule);break;
            }
        }
        function drawImage(id,x,y, width, height) {
            if (!currentCanvas.getContext)
                return;
            var ctx = currentCanvas.getContext('2d');
            var img = document.getElementById(id);
            ctx.drawImage(img, x, y,width,height);  
        }       
    </script>");
        }

        protected void AppendColor(ColorDetails color, PdfHtmlWriterContext sb)
        {
            switch (color)
            {
                case GardientColorDetails gardientColor:
                    sb.Append('[').Append(gardientColor.Start.AbsoluteX)
                        .Append(',').Append(gardientColor.Start.AbsoluteY)
                        .Append(',').Append(gardientColor.End.AbsoluteX)
                        .Append(',').Append(gardientColor.End.AbsoluteY);
                    foreach (var c in gardientColor.Colors)
                    {
                        sb.Append(',').Append(c.OffSet).Append(",'");
                        PdfHtmlWriter.AppendColor(c.Color, sb);
                        sb.Append("'");

                    }
                    sb.Append(']');
                        
                    break;
                case SimpleColorDetails simpleColor:
                    sb.Append("'");
                    PdfHtmlWriter.AppendColor(simpleColor.Color, sb);
                    sb.Append("'");
                    break;
                default: sb.Append("null");
                    break;
            }
        }

        public PdfDrawCanvasHtmlWriter(HtmlWriterConfig config) : base(config)
        {
        }
    }
}