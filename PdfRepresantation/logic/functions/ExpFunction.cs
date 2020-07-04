using System;
using System.Collections.Generic;
using System.Linq;
using iText.Kernel.Pdf;

namespace PdfRepresantation
{
    public class ExpFunction : Function
    {
        private readonly float[] c0;
        private readonly float[] c1;
        private readonly float n;

        public ExpFunction(PdfDictionary dict) : base(dict)
        {
            c0 = dict.GetAsArray(PdfName.C0)?.ToFloatArray()??new []{0F};
            c1 = dict.GetAsArray(PdfName.C1)?.ToFloatArray()??new []{1F};
            n = dict.GetAsFloat(PdfName.N).Value;
        }

        protected override float[] CalculateImplemantaion(float[] inputs)
        {
            var input =Math.Pow(inputs[0], n) ;
            var output = new float[c0.Length];
            for (var i = 0; i < output.Length; i++)
            {
                output[i]=  (float) (c0[i] + input* (c1[i] - c0[i]));
            }
            return output;
        }

        public override int Type => 2;
        public override IEnumerable<float> PointsControl =>
            n==1?new []{0F,1F}:
            Enumerable.Range(0, 11)
            .Select(i => i / 10F);
    }
}