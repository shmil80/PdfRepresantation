﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Colorspace;
using Color = System.Drawing.Color;

namespace PdfRepresantation
{
    /// <summary>
    /// This class try to get the details from gradient color of pdf which are nessecary for to html and image.</br>
    /// Since there are many difference between the way pdf and others draw a gradient this is not easy at all.
    /// I did my best here, and uit's not complete at all.
    /// </summary>
    public class ColorManagerPattern : ColorManager
    {
        public static ColorManagerPattern Instance = new ColorManagerPattern();

        protected override ColorSpace Type => ColorSpace.Pattern;

        public override ColorDetails GetColorDetails(PdfPage page, iText.Kernel.Colors.Color colorPfd, float alpha)
        {
            return GetColor(page, (colorPfd as PatternColor)?.GetPattern(), alpha);
        }

        internal override Color? Color(iText.Kernel.Colors.Color colorPfd, float alpha)
        {
            throw new NotSupportedException();
        }

        public override int LengthColor(PdfObject o)
        {
            return -1;
        }

        private GardientColorDetails GetColor(PdfPage page, PdfPattern pattern, float alpha)
        {
            //not supported yet
            // return null;
            switch (pattern)
            {
                case null: return null;
                case PdfPattern.Shading shading:
                    return GetColor(page, shading, alpha);
                case PdfPattern.Tiling tiling:
                    return GetColor(page, tiling, alpha);
                default: throw new IndexOutOfRangeException();
            }
        }


        private IList<ColorInGardient> GetColorsFromFunction(NormalColorManager colorManager,
            PdfObject pdfFunction, float alpha)
        {
            var result = new List<ColorInGardient>();
            IList<float> pointBreaks;
            void AddItem(float[] values,float offset)
            {
                var color = colorManager.Color(values, alpha);
                if (color.HasValue)
                {
                    result.Add(new ColorInGardient(color.Value, offset));
                }

            }
            switch (pdfFunction)
            {
                case PdfDictionary dictFunc:
                    var function = Function.Create(dictFunc);
                    pointBreaks = function.PointsControl.ToArray();
                    for (var index = 0; index < pointBreaks.Count; index++)
                    {
                        var offset = pointBreaks[index];
                        var values = function.Calculate(new[] {offset});
                        AddItem(values, offset);
                    }

                    break;
                case PdfArray arrayFunc:
                    var functions = arrayFunc
                        .Select(d => Function.Create((PdfDictionary) d))
                        .ToArray();
                    pointBreaks = functions[0].PointsControl.ToArray();
                    for (var index = 0; index < pointBreaks.Count; index++)
                    {
                        var offset = pointBreaks[index];
                        var values = functions
                            .Select(f => f.Calculate(new[] {offset})[0])
                            .ToArray();
                        AddItem(values, offset);
                    }

                    break;
            }

            return result;
        }

        public static void CalculteRelativePosition(GardientColorDetails gradient,
            float minX, float minY, float maxX, float maxY)
        {
            CalculteRelativePosition(gradient.Start, minX, minY, maxX, maxY);
            CalculteRelativePosition(gradient.End, minX, minY, maxX, maxY);
        }

        private static void CalculteRelativePosition(GardientPoint point, float minX, float minY, float maxX, float maxY)
        {
            point.RelativeX = (point.AbsoluteX - minX) / (maxX - minX);
            point.RelativeY = (point.AbsoluteY - minY) / (maxY - minY);
        }


        private GardientColorDetails GetColor(PdfPage page, PdfPattern.Shading shading, float alpha)
        {
            var shadingDetailsDict = shading.GetShading();
            var shadingDetails = PdfShading.MakeShading(shadingDetailsDict);
            if (!(GetManagerBySpace(shadingDetails.GetColorSpace()) is NormalColorManager colorManager))
                return null;
            GardientColorDetails result = new GardientColorDetails();

            var pdfFunction = shadingDetails.GetFunction();
            var pointsBreak = new[] {0F, 1F};
            var colors = GetColorsFromFunction(colorManager, pdfFunction, alpha);
            result.Colors = colors;


            var coordsArray = shadingDetails.GetPdfObject().GetAsArray(PdfName.Coords);
            if (coordsArray != null)
            {
                var coords = coordsArray.ToFloatArray();
                result.Start = new GardientPoint
                {
                    AbsoluteX = coords[0],
                    AbsoluteY = page.GetPageSize().GetHeight() - coords[1]
                };

                result.End = new GardientPoint
                {
                    AbsoluteX = coords[2],
                    AbsoluteY = page.GetPageSize().GetHeight() - coords[3]
                };
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

        private static GardientColorDetails GetColor(PdfPage page,PdfPattern.Tiling tiling, float alpha)
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