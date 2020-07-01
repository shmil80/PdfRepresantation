using System.Collections.Generic;
using System.Linq;

namespace PdfRepresantation.postScript
{
    public class CodeExecution : ValueOperand
    {
        internal readonly List<Operator> Code = new List<Operator>();

        public IList<ValueOperand> Execute(params ValueOperand[] parameters)
        {
            Stack<Operand> stack = new Stack<Operand>(parameters);
            foreach (var @operator in Code)
            {
                @operator.Apply(stack);
            }

            return stack.Cast<ValueOperand>().ToArray();
        }
    }
}