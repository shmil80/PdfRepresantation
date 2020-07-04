using System;
using System.Collections.Generic;
using System.Linq;
using iText.Kernel.Pdf;

namespace PdfRepresantation
{
    public class SampledFunction : Function
    {
        private readonly Range[] encodes;
        private readonly Range[] decodes;
        private readonly int[] sizes;
        private readonly int[] samples;
        private readonly Range sampleRange;

        public SampledFunction(PdfStream dict) : base(dict)
        {
            sizes = dict.GetAsArray(PdfName.Size).ToIntArray();
            var bitPerSample = dict.GetAsInt(PdfName.BitsPerSample).Value;
            this.sampleRange = new Range {Max = 2 ^ bitPerSample - 1};
            dict.GetAsInt(PdfName.Order);
            var encode = dict.GetAsArray(PdfName.Encode)?.ToFloatArray();
            if (encode == null)
            {
                encodes = sizes
                    .Select(s => new Range {Max = s - 1})
                    .ToArray();
            }
            else
            {
                encodes = Range.CreateArray(encode);
            }

            var decode = dict.GetAsArray(PdfName.Decode)?.ToFloatArray();
            if (decode == null)
            {
                decodes = OutputsRange;
            }
            else
            {
                decodes = Range.CreateArray(decode);
            }

            var sampleData = dict.GetBytes(true);
            samples = SampleBitConverter.ConvertBits(sampleData, bitPerSample);
        }


        protected override float[] CalculateImplemantaion(float[] inputs)
        {
            var outputs = new float[OutputsRange.Length];
            int index = 0;
            int multi = 1;
            for (int m = 0; m < inputs.Length; m++)
            {
                var e = (int) Math.Round(Range.Interpolate(inputs[m], InputsRange[m], encodes[m]));
                if (e < 0) e = 0;
                else if (e >= sizes[m]) e = sizes[m] - 1;
                index += (e) * multi;
                multi *= sizes[m];
            }

            for (int n = 0; n < outputs.Length; n++)
            {
                outputs[n] = samples[index + n * multi];
                outputs[n] = Range.Interpolate(outputs[n], sampleRange, decodes[n]);
            }

            return outputs;
        }


        public override int Type => 0;
        public override IEnumerable<float> PointsControl => new[] {0F, 1F};
    }
}