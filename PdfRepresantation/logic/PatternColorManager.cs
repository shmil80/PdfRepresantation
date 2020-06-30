using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Colorspace;
using iText.Kernel.Pdf.Function;

namespace PdfRepresantation
{
    class PatternColorManager
    {
        public static GardientColorDetails GetColor(PdfPattern pattern, float alpha)
        {
            //not supported yet
           // return null;
            switch (pattern)
            {
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
                    GetFunctionDetails(axialFunction);
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


 
        private static void GetFunctionDetails(PdfDictionary dict)
        {
            var function = new PdfFunction(dict);
            int[] domain;
            int[] encode;
            switch (function.GetFunctionType())
            {
                case 0:
                    domain = dict.GetAsArray(PdfName.Domain).ToIntArray();
                    dict.GetAsArray(PdfName.Size).ToIntArray();
                    dict.GetAsInt(PdfName.BitsPerSample);
                    dict.GetAsInt(PdfName.Order);
                    encode = dict.GetAsArray(PdfName.Encode).ToIntArray();
                   var decode = dict.GetAsArray(PdfName.Decode).ToIntArray();
                   var range = dict.GetAsArray(PdfName.Range).ToIntArray();
                    break;
                case 2:
                    domain = dict.GetAsArray(PdfName.Domain).ToIntArray();
                    var color1 = dict.GetAsArray(PdfName.C0).ToFloatArray();
                    var color2 = dict.GetAsArray(PdfName.C1).ToFloatArray();
                    var n=dict.GetAsInt(PdfName.N);
                    break;
                case 3:
                    domain = dict.GetAsArray(PdfName.Domain).ToIntArray();
                    foreach (PdfDictionary sub in dict.GetAsArray(PdfName.Functions))
                    {
                        GetFunctionDetails(sub);
                    }

                    var bounds = dict.GetAsArray(PdfName.Bounds).ToFloatArray();
                    encode = dict.GetAsArray(PdfName.Encode).ToIntArray();
                    break;
                case 4:
                    domain = dict.GetAsArray(PdfName.Domain).ToIntArray();
                    break;
            }
        }
    }
}