using System;
using System.Linq;
using iText.Kernel.Pdf;

namespace PdfRepresantation
{
    public abstract class Function
    {
        public static Function Create(PdfDictionary dict)
        {
            switch (dict?.GetAsInt(PdfName.FunctionType))
            {
                case 0: return new SampledFunction(dict as PdfStream);
                case 2: return new ExpFunction(dict);
                case 3: return new StitchFunction(dict);
                case 4: return new PostScriptFunction(dict as PdfStream);
            }

            return null;
        }

        public readonly Range[] InputsRange;
        public readonly Range[] OutputsRange;

        public float[] Calculate(float[] inputs)
        {
            if (inputs.Length != InputsRange.Length)
                throw new ArgumentException();
            inputs = inputs
                .Select((input, i) => InputsRange[i].Cut(input))
                .ToArray();
            var outputs = CalculateImplemantaion(inputs);
            if (OutputsRange != null)
                outputs = outputs
                    .Select((output, i) => OutputsRange[i].Cut(output))
                    .ToArray();
            return outputs;
        }

        protected abstract float[] CalculateImplemantaion(float[] inputs);

        protected Function(PdfDictionary dict)
        {
            var domain = dict.GetAsArray(PdfName.Domain).ToFloatArray();
            InputsRange = Range.CreateArray(domain);
            var range = dict.GetAsArray(PdfName.Range)?.ToFloatArray();
            if (range != null)
                OutputsRange = Range.CreateArray(range);
        }

        public abstract int Type { get; }
    }
}