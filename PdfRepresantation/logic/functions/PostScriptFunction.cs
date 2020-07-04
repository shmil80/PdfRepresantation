using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iText.Kernel.Pdf;
using PdfRepresantation.postScript;

namespace PdfRepresantation
{
    public class PostScriptFunction : Function
    {
        private readonly CodeExecution code;

        public PostScriptFunction(PdfStream dict) : base(dict)
        {
            code=new PostScriptParser().Parse(Encoding.Default.GetString(dict.GetBytes()));
        }

        protected override float[] CalculateImplemantaion(float[] inputs)
        {
            var args = inputs
                .Select(f =>(ValueOperand) new NumberOperand(f))
                .ToArray();
            var result = code.Execute(args);
            return result
                .Cast<NumberOperand>()
                .Select(n => n.FloatValue).ToArray();
        }

        public override int Type => 4;
        public override IEnumerable<float> PointsControl => Enumerable.Range(0, 11)
            .Select(i => i / 10F);

    }

    
}