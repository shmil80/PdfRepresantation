using System;
using System.Collections.Generic;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace PdfRepresantation
{
    public class TextParser
    {
        public readonly IList<PdfTextBlock> texts = new List<PdfTextBlock>();
        public readonly Dictionary<PdfFont, PdfFontDetails> fonts = new Dictionary<PdfFont, PdfFontDetails>();
        private readonly PageContext pageContext;

        internal TextParser(PageContext pageContext)
        {
            this.pageContext = pageContext;
        }

        class RectangleRotated
        {
            public float Left, Bottom, Width, Height, Angle,BottomInOwnPlane;

            public RectangleRotated(Vector leftBottom, Vector rightBottom,
                Vector leftTop, Vector rightTop)
            {
                Left = leftBottom.Get(Vector.I1);
                Bottom = leftBottom.Get(Vector.I2);

                var cosLine = rightBottom.Get(Vector.I1) - leftBottom.Get(Vector.I1);
                var sinLine = rightBottom.Get(Vector.I2) - leftBottom.Get(Vector.I2);
                var heightRectLeftLine = leftTop.Get(Vector.I2) - leftBottom.Get(Vector.I2);
                if (Math.Abs(sinLine) < 0.0001)
                {
                    Height = heightRectLeftLine;
                    if (cosLine < 0)
                    {
                        Angle = 180;
                        Width = -cosLine;
                        BottomInOwnPlane = -Bottom;
                    }
                    else
                    {
                        Angle = 0;
                        Width = cosLine;
                        BottomInOwnPlane = Bottom;
                    }

                    return;
                }

                if (Math.Abs(cosLine) < 0.0001)
                {
                    var widthRectLeftLine = leftTop.Get(Vector.I1) - leftBottom.Get(Vector.I1);
                    if (sinLine < 0)
                    {
                        Angle = 90;
                        Width = -sinLine;
                        Height = widthRectLeftLine;
                        BottomInOwnPlane = Left;
                    }
                    else
                    {
                        Angle = 270;
                        Width = sinLine;
                        Height = -widthRectLeftLine;
                        BottomInOwnPlane = -Left;
                    }

                    return;
                }


                var radians = Math.Atan2(cosLine, sinLine);
                Angle = (float) (radians * 180 / Math.PI);

                var ration = Math.Cos(radians);
                Width = (float) (cosLine / ration);
                ration = Math.Sin(radians);
                Height = (float) (heightRectLeftLine / ration);
                
                var startPageRadian=Math.Atan2(Left, Bottom);
                var lineFromStartPage = Math.Sqrt(Left * Left + Bottom * Bottom);
                BottomInOwnPlane = (float) (Math.Sin(startPageRadian + radians) * lineFromStartPage);
            }
        }

        public virtual void ParseText(TextRenderInfo textRenderInfo)
        {
            var text = textRenderInfo.GetText();
            LineSegment baseline = textRenderInfo.GetBaseline();
            LineSegment ascentLine = textRenderInfo.GetAscentLine();
            if (textRenderInfo.GetRise() != 0)
            {
                Matrix m = new Matrix(0.0f, -textRenderInfo.GetRise());
                baseline = baseline.TransformBy(m);
                ascentLine = ascentLine.TransformBy(m);
            }


            var position = new RectangleRotated(baseline.GetStartPoint(),
                baseline.GetEndPoint(),
                ascentLine.GetStartPoint(), ascentLine.GetEndPoint());
            PdfTextBlock item = new PdfTextBlock
            {
                Value = text,
                Bottom = pageContext.PageHeight - position.Bottom,
                Left = position.Left,
                Width = position.Width,
                Height = position.Height,
                Rotation = position.Angle,
                BottomInOwnPlane = pageContext.PageHeight -position.BottomInOwnPlane,
                StrokeColore = ColorManager.Instance.GetColor(textRenderInfo),
                CharSpacing = textRenderInfo.GetSingleSpaceWidth(),
                Font = GetFont(textRenderInfo),
            };
            item.FontSize = FontManager.Instance.GetFontSize(textRenderInfo, item);
            RightToLeftManager.Instance.AssignRtl(item, textRenderInfo.GetUnscaledWidth() < 0);
            pageContext.LinkManager.AssignLink(item);
            texts.Add(item);
        }

        private PdfFontDetails GetFont(TextRenderInfo textRenderInfo)
        {
            var pdfFont = textRenderInfo.GetFont();
            if (!fonts.TryGetValue(pdfFont, out var font))
            {
                font = FontManager.Instance.CreateFont(pdfFont);
                fonts.Add(pdfFont, font);
            }

            return font;
        }
    }
}