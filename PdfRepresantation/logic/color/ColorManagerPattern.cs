using System;
using System.Linq;
using System.Runtime.CompilerServices;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Colorspace;
using Color = System.Drawing.Color;

namespace PdfRepresantation
{
    public class ColorManagerPattern : ColorManager
    {
        public static ColorManagerPattern Instance = new ColorManagerPattern();

        protected override ColorSpace Type => ColorSpace.Pattern;

        public override ColorDetails GetColorDetails(iText.Kernel.Colors.Color colorPfd, float alpha)
        {
            return GetColor((colorPfd as PatternColor)?.GetPattern(), alpha);
        }

        internal override Color? Color(iText.Kernel.Colors.Color colorPfd, float alpha)
        {
            throw new NotSupportedException();
        }

        public override int LengthColor(PdfObject o)
        {
            return -1;
        }

        private GardientColorDetails GetColor(PdfPattern pattern, float alpha)
        {
            //not supported yet
            // return null;
            switch (pattern)
            {
                case null: return null;
                case PdfPattern.Shading shading:
                    return GetColor(shading, alpha);
                case PdfPattern.Tiling tiling:
                    return GetColor(tiling, alpha);
                default: throw new IndexOutOfRangeException();
            }
        }

        private GardientColorDetails GetColor(PdfPattern.Shading shading, float alpha)
        {
            var matrix = shading.GetMatrix();
            var shadingDetailsDict = shading.GetShading();
            var shadingDetails = PdfShading.MakeShading(shadingDetailsDict);
            var pdfFunction = (PdfDictionary) shadingDetails.GetFunction();
            var function = Function.Create(pdfFunction);
            var colorManager = GetManagerBySpace(shadingDetails.GetColorSpace());
            var background = shadingDetails.GetPdfObject().GetAsArray(new PdfName("background"))?.ToFloatArray();
            float[] domain, coords;
            bool[] extend;
            switch (shadingDetails)
            {
                case PdfShading.Axial axial:
                    return GetAxialColor(axial);
                case PdfShading.Radial radial:
                    coords = radial.GetCoords().ToFloatArray();
                    domain = radial.GetDomain().ToFloatArray();
                    extend = radial.GetExtend().ToBooleanArray();
                    break;
                case PdfShading.FunctionBased functionBased:
                    FunctionBasedColor(functionBased);
                    break;
                case PdfShading.CoonsPatchMesh coonsPatchMesh:
                    break;
                case PdfShading.FreeFormGouraudShadedTriangleMesh freeFormGouraudShadedTriangleMesh: break;
                case PdfShading.LatticeFormGouraudShadedTriangleMesh latticeFormGouraudShadedTriangleMesh:
                    break;
                case PdfShading.TensorProductPatchMesh tensorProductPatchMesh: break;
            }

            return null;
        }

        private static GardientColorDetails GetAxialColor(PdfShading.Axial axial1)
        {
            float[] coords;
            float[] domain;
            bool[] extend;
            coords = axial1.GetCoords().ToFloatArray();
            domain = axial1.GetDomain().ToFloatArray();
            extend = axial1.GetExtend().ToBooleanArray();
            return null;
        }

        private static void FunctionBasedColor(PdfShading.FunctionBased functionBased)
        {
            float[] domain = functionBased.GetDomain().ToFloatArray();
            var pdfFunction = functionBased.GetFunction();
            if (pdfFunction is PdfDictionary dictFunc)
            {
                var function = Function.Create(dictFunc);
            }
            else if (pdfFunction is PdfArray arrayFunc)
            {
                var functions = arrayFunc
                    .Select(d => Function.Create((PdfDictionary) d))
                    .ToArray();
            }
        }

        private static GardientColorDetails GetColor(PdfPattern.Tiling tiling, float alpha)
        {
            switch (tiling.GetTilingType())
            {
                case PdfPattern.Tiling.TilingType.CONSTANT_SPACING:
                case PdfPattern.Tiling.TilingType.CONSTANT_SPACING_AND_FASTER_TILING:
                case PdfPattern.Tiling.TilingType.NO_DISTORTION:
                    break;
            }

            var box = tiling.GetBBox();
            var xStep = tiling.GetXStep();
            var yStep = tiling.GetYStep();
            var colored = tiling.IsColored();
            return null;
        }
    }
}