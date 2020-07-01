using System.Collections.Generic;

namespace PdfRepresantation.postScript
{
    class IfOperator : Operator
    {
        public override void Apply(Stack<Operand> stack)
        {
            var condition = (BoolOperand) stack.Pop();
            var block = (CodeExecution) stack.Pop();
            if (condition.Value)
                foreach (var @operator in block.Code)
                {
                    @operator.Apply(stack);
                }
        }
    }
    class IfElseOperator : Operator
    {
        public override void Apply(Stack<Operand> stack)
        {
            var condition = (BoolOperand) stack.Pop();
            var blockFalse = (CodeExecution) stack.Pop();
            var blockTrue = (CodeExecution) stack.Pop();
            var block = condition.Value ? blockTrue : blockFalse;
            foreach (var @operator in block.Code)
            {
                @operator.Apply(stack);
            }
        }
    }
}