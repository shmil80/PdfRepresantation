using System;
using System.Linq;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Filters;
using iText.Kernel.Pdf.Xobject;
using Color = System.Drawing.Color;

namespace PdfRepresantation
{
    public class ColorManager
    {
        public static readonly ColorManager Instance = new ColorManager();

        internal ColorSpace GetSpaceByType(iText.Kernel.Colors.Color colorPfd)
        {
            switch (colorPfd)
            {
                case CalGray _: return ColorSpace.CalGray;
                case DeviceGray _: return ColorSpace.DeviceGray;
                case CalRgb _: return ColorSpace.CalRgb;
                case DeviceRgb _: return ColorSpace.DeviceRGB;
                case DeviceCmyk _: return ColorSpace.DeviceCMYK;
                case IccBased _: return ColorSpace.IccBased;
                case PatternColor _: return ColorSpace.Pattern;
                case DeviceN _: return ColorSpace.DeviceN;
                case Indexed _: return ColorSpace.Indexed;
                case Lab _: return ColorSpace.Lab;
                case Separation _: return ColorSpace.Separation;
                default: throw new IndexOutOfRangeException();
            }
        }

        internal ColorSpace GetSpaceByName(PdfName name)
        {
            if (name.Equals(PdfName.CalGray)) return ColorSpace.CalGray;
            if (name.Equals(PdfName.DeviceGray)) return ColorSpace.DeviceGray;
            if (name.Equals(PdfName.CalRGB)) return ColorSpace.CalRgb;
            if (name.Equals(PdfName.DeviceRGB)) return ColorSpace.DeviceRGB;
            if (name.Equals(PdfName.DeviceCMYK)) return ColorSpace.DeviceCMYK;
            if (name.Equals(PdfName.ICCBased)) return ColorSpace.IccBased;
            if (name.Equals(PdfName.Pattern)) return ColorSpace.Pattern;
            if (name.Equals(PdfName.Indexed)) return ColorSpace.Indexed;
            if (name.Equals(PdfName.Lab)) return ColorSpace.Lab;
            if (name.Equals(PdfName.Separation)) return ColorSpace.Separation;
            throw new IndexOutOfRangeException();
        }

        public Color? GetColorBySpace(ColorSpace space, float[] value, float alpha)
        {
            switch (space)
            {
                case ColorSpace.DeviceGray:
                case ColorSpace.CalGray:
                    return FromGray(value[0], alpha);
                case ColorSpace.DeviceRGB:
                case ColorSpace.CalRgb:
                case ColorSpace.Lab:
                    return FromRGB(value[0], value[1], value[2], alpha);
                case ColorSpace.DeviceCMYK:
                    return FromCmyk(value[0], value[1], value[2], value[3], alpha);
                case ColorSpace.Separation:
                case ColorSpace.DeviceN:
                case ColorSpace.Pattern:
                case ColorSpace.Indexed:
                case ColorSpace.IccBased:
                    return FromUnkown(value, alpha);
                default: throw new ArgumentOutOfRangeException(nameof(space), space, null);
            }
        }

        internal ColorDetails GetColor(iText.Kernel.Colors.Color colorPfd, float alpha)
        {
            var value = colorPfd.GetColorValue();
            var type = GetSpaceByType(colorPfd);
            Color? color;
            switch (type)
            {
                case ColorSpace.Pattern:
                    return PatternColorManager.GetColor(((PatternColor) colorPfd).GetPattern(), alpha);
                case ColorSpace.IccBased:
                case ColorSpace.Separation:
                case ColorSpace.DeviceN:
                    var colorSpace = colorPfd.GetColorSpace();
                    var source = ((PdfArray) colorSpace.GetPdfObject()).GetAsStream(1);
                    var altName = source.GetAsName(PdfName.Alternate);
                    if (altName != null)
                    {
                        var altType = GetSpaceByName(altName);
                        color = GetColorBySpace(altType, value, alpha);
                    }
                    else
                    {
                        color = FromUnkown(value, alpha);
                    }

                    if (color == null)
                        return null;
                    return new SimpleColorDetails{Space = type,Color = color.Value};
                    ;
                default:
                    color = GetColorBySpace(type, value, alpha);
                    if (color == null)
                        return null;
                    return new SimpleColorDetails
                    {
                        Space = type,
                        Color = color.Value
                    };
            }
        }

        public Color? GetColorBySpace(ColorSpace space, int[] value)
        {
            switch (space)
            {
                case ColorSpace.DeviceGray:
                case ColorSpace.CalGray:
                    return Color.FromArgb(value[0], value[0], value[0]);
                case ColorSpace.DeviceRGB:
                case ColorSpace.CalRgb:
                case ColorSpace.Lab:
                    return Color.FromArgb(value[0], value[1], value[2]);
                case ColorSpace.DeviceCMYK:
                    return FromCmyk(value[0], value[1], value[2], value[3]);
                case ColorSpace.Separation:
                case ColorSpace.DeviceN:
                case ColorSpace.Pattern:
                case ColorSpace.Indexed:
                case ColorSpace.IccBased:
                    return FromUnkown(value);
                default: throw new ArgumentOutOfRangeException(nameof(space), space, null);
            }
        }

        private Color? FromUnkown(float[] value, float alpha)
        {
            switch (value.Length)
            {
                case 0: return null;
                case 1: return FromGray(value[0], alpha);
                case 3: return FromRGB(value[0], value[1], value[2], alpha);
                case 4: return FromCmyk(value[0], value[1], value[2], value[3], alpha);
                default: return null;
            }
        }

        private Color? FromUnkown(int[] value)
        {
            switch (value.Length)
            {
                case 0: return null;
                case 1: return Color.FromArgb(value[0]);
                case 3: return Color.FromArgb(value[0], value[1], value[2]);
                case 4: return FromCmyk(value[0], value[1], value[2], value[3]);
                default: return null;
            }
        }

        private Color FromRGB(float red, float green, float blue, float alpha)
        {
            return Color.FromArgb((int) (alpha * 255),
                (int) (red * 255),
                (int) (green * 255),
                (int) (blue * 255));
        }

        private static Color FromGray(float gray, float alpha)
        {
            var g = (int) (gray * 255);
            return Color.FromArgb((int) (alpha * 255), g, g, g);
        }

        private static Color FromCmyk(float c, float m, float y, float k, float alpha)
        {
            var r = (int) (255 * (1 - c) * (1 - k));
            var g = (int) (255 * (1 - m) * (1 - k));
            var b = (int) (255 * (1 - y) * (1 - k));
            return Color.FromArgb((int) (alpha * 255), r, g, b);
        }

        private static Color FromCmyk(int c, int m, int y, int k)
        {
            return FromCmyk(c / 255F, m / 255F, y / 255F, k / 255F, 0);
        }

        public TextRenderDetails GetColor(TextRenderInfo text)
        {
            var gs = text.GetGraphicsState();
            var result = new TextRenderDetails
            {
                Option = GetOption(text),
                FillColor = GetColor(text.GetFillColor(), gs.GetFillOpacity()),
                StrokeColor = GetColor(text.GetStrokeColor(), gs.GetStrokeOpacity())
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

        public Color? ChooseMain(TextRenderDetails result)
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