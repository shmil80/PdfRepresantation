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
            var shadingDetailsDict = shading.GetShading();
            var shadingDetails = PdfShading.MakeShading(shadingDetailsDict);
            var pdfFunction = shadingDetails.GetFunction();
            var colorManager = GetManagerBySpace(shadingDetails.GetColorSpace()) as NormalColorManager;
            if (colorManager == null)
                return null;
            GardientColorDetails result=new GardientColorDetails();
            var coordsArray=shadingDetails.GetPdfObject().GetAsArray(PdfName.Coords);
            if (coordsArray != null)
            {
                var coords = coordsArray.ToFloatArray();
                result.Start=new ShapePoint{X = coords[0],Y = coords[1]};
                result.End=new ShapePoint{X = coords[2],Y = coords[3]};
            }
                    var arr0 = new[] {0F};
                    var arr1 = new[] {1F};
            switch (pdfFunction)
            {
                case PdfDictionary dictFunc:
                    var function = Function.Create(dictFunc);
                    result.ColorStart = colorManager.Color(function.Calculate(arr0), alpha);
                    result.ColorEnd = colorManager.Color(function.Calculate(arr1), alpha);
                    break;
                case PdfArray arrayFunc:
                    var functions = arrayFunc
                        .Select(d => Function.Create((PdfDictionary) d))
                        .ToArray();
                    result.ColorStart = colorManager.Color(functions.Select(f=>f.Calculate(arr0)[0]).ToArray(), alpha);
                    result.ColorEnd = colorManager.Color(functions.Select(f=>f.Calculate(arr0)[1]).ToArray(), alpha);
                    break;
            }

            return result;
        }

        //not supported yet
        private GardientColorDetails GetFullColor(PdfPattern.Shading shading, float alpha)
        {
            var matrix = shading.GetMatrix();
            var shadingDetailsDict = shading.GetShading();
            var shadingDetails = PdfShading.MakeShading(shadingDetailsDict);
            var pdfFunction = (PdfDictionary) shadingDetails.GetFunction();
            var function = Function.Create(pdfFunction);
            var colorManager = GetManagerBySpace(shadingDetails.GetColorSpace());
            var background = shadingDetails.GetPdfObject().GetAsArray(new PdfName("background"))?.ToFloatArray();
            switch (shadingDetails)
            {
                case PdfShading.Axial axial:
                    return GetAxialColor(axial);
                case PdfShading.Radial radial:
                    return GetRadialColor(radial);
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

        private static GardientColorDetails GetAxialColor(PdfShading.Axial axial)
        {
            var coords = axial.GetCoords().ToFloatArray();
            var domain = axial.GetDomain().ToFloatArray();
            var extend = axial.GetExtend().ToBooleanArray();
            BuildFunction(axial);
            return null;
        }
        private static GardientColorDetails GetRadialColor(PdfShading.Radial radial)
        {
            var coords = radial.GetCoords().ToFloatArray();
            var domain = radial.GetDomain().ToFloatArray();
            var extend = radial.GetExtend().ToBooleanArray();
            BuildFunction(radial);
            return null;
        }

        private static void FunctionBasedColor(PdfShading.FunctionBased functionBased)
        {
            float[] domain = functionBased.GetDomain().ToFloatArray();
            BuildFunction(functionBased);
        }

        private static void BuildFunction(PdfShading functionBased)
        {
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