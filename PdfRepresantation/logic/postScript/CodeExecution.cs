using System.Collections.Generic;
using System.Linq;

namespace PdfRepresantation.postScript
{
    public class CodeExecution : ValueOperand
    {
        internal readonly List<Operator> Code = new List<Operator>();

        public IList<ValueOperand> Execute(params ValueOperand[] parameters)
        {
            var stack = new Stack<Operand>(parameters);
            Execute(stack);
            return stack.Cast<ValueOperand>().ToArray();
        }

        public void Execute(Stack<Operand> stack)
        {
            foreach (var @operator in Code)
            {
                @operator.Apply(stack);
            }
        }
    }
}