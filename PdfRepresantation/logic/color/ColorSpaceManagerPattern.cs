using System;
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

        public override Color? Color(int[] value, float alpha)
        {
            throw new NotSupportedException();
        }

        public override int LengthColor(PdfObject o)
        {
            return -1; }

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

        private static GardientColorDetails GetColor(PdfPattern.Shading shading, float alpha)
        {
            var shadingDict = shading.GetShading();
            var shadingConstructed = PdfShading.MakeShading(shadingDict);
            switch (shadingConstructed)
            {
                case PdfShading.Axial axial:
                    var axialFunction = (PdfDictionary) axial.GetFunction();
                    FunctionColorManager.GetFunctionDetails(axialFunction);
                    var coords = axial.GetCoords().ToFloatArray();
                    var domain = axial.GetDomain().ToFloatArray();
                    var extend = axial.GetExtend().ToBooleanArray();

                    break;
                case PdfShading.CoonsPatchMesh coonsPatchMesh:
                    break;
                case PdfShading.FreeFormGouraudShadedTriangleMesh freeFormGouraudShadedTriangleMesh: break;
                case PdfShading.FunctionBased functionBased: break;
                case PdfShading.LatticeFormGouraudShadedTriangleMesh latticeFormGouraudShadedTriangleMesh:
                    break;
                case PdfShading.Radial radial: break;
                case PdfShading.TensorProductPatchMesh tensorProductPatchMesh: break;
            }

            return null;
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