using System;
using System.Collections.Generic;
using System.Linq;
using iText.Kernel.Pdf;

namespace PdfRepresantation
{
    public class StitchFunction : Function
    {
        private readonly Function[] functions;
        private readonly Range[] bounds;
        private readonly Range[] encodes;

        public StitchFunction(PdfDictionary dict) : base(dict)
        {
            functions = dict.GetAsArray(PdfName.Functions)
                .Cast<PdfDictionary>()
                .Select(Create)
                .ToArray();

            var boundsArray = dict.GetAsArray(PdfName.Bounds);
            bounds = new Range[functions.Length];
            var domain = InputsRange[0];
            float last = domain.Min;
            for (var i = 0; i < bounds.Length - 1; i++)
            {
                var min = last;
                last = boundsArray.GetAsNumber(i).FloatValue();
                bounds[i] = new Range {Min = min, Max = last};
            }
            bounds[bounds.Length - 1] = new Range {Min = last, Max = domain.Max};
            var encode = dict.GetAsArray(PdfName.Encode).ToFloatArray();
            encodes = Range.CreateArray(encode);
        }

        public override IEnumerable<float> PointsControl
        {
            get
            {
                if(bounds.Length==0)
                    yield break;
                var current = bounds[0].Min;
                for (var index = 0; index < bounds.Length; index++)
                {
                    var bound = bounds[index];
                    var function = functions[index];
                    var start = bound.Min;
                    var length = bound.Length;
                    foreach (var point in function.PointsControl.Skip(1))
                    {
                        yield return current;
                        current = start + point * length;
                    }
                }

                yield return bounds.Last().Max;
            }
        }

        protected override float[] CalculateImplemantaion(float[] inputs)
        {
            var input = inputs[0];
            int i=0;
            for (i = 0; i < bounds.Length; i++)
            {
                if (bounds[i].Max >= input)
                    break;
            }

            if (i == bounds.Length)
                i--;
            inputs[0] = Range.Interpolate(input, bounds[i], encodes[i]);
            return functions[i].Calculate(inputs);
        }

        public override int Type => 3;
    }
}