using System;
using System.Drawing;
using iText.Kernel.Pdf.Canvas.Parser.Data;

namespace PdfRepresantation
{
    class TextRenderManager
    {
        public static TextRenderDetails GetColor(TextRenderInfo text)
        {
            var gs = text.GetGraphicsState();
            var result = new TextRenderDetails
            {
                Option = GetOption(text),
                FillColor = ColorManager.GetColor(text.GetFillColor(), gs.GetFillOpacity()),
                StrokeColor = ColorManager.GetColor(text.GetStrokeColor(), gs.GetStrokeOpacity())
            };
            result.MainColor = ChooseMain(result);
            return result;
        }

        private static TextRenderOptions GetOption(TextRenderInfo text)
        {
            switch (text.GetTextRenderMode())
            {
                case 0: return TextRenderOptions.Fill;
                case 1: return TextRenderOptions.Stroke;
                case 2: return TextRenderOptions.Fill_Stroke;
                case 3: return TextRenderOptions.Invisible;
                case 4: return TextRenderOptions.Fill_Path;
                case 5: return TextRenderOptions.Stroke_Path;
                case 6: return TextRenderOptions.Fill_Stroke_Path;
                case 7: return TextRenderOptions.Path;
                default: throw new ApplicationException("Wrong render mode");
            }
        }

        public static Color? ChooseMain(TextRenderDetails result)
        {
            switch (result.Option)
            {
                case TextRenderOptions.Invisible: return Color.Transparent;
                case TextRenderOptions.Fill:
                case TextRenderOptions.Fill_Path: return (result.FillColor as SimpleColorDetails)?.Color;
                case TextRenderOptions.Stroke:
                case TextRenderOptions.Stroke_Path: return (result.StrokeColor as SimpleColorDetails)?.Color;
                case TextRenderOptions.Path:
                case TextRenderOptions.Fill_Stroke:
                case TextRenderOptions.Fill_Stroke_Path:
                    if (result.StrokeColor is SimpleColorDetails strokeSimple
                        && strokeSimple.Color.A != 0)
                        return strokeSimple.Color;
                    return (result.FillColor as SimpleColorDetails)?.Color;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}