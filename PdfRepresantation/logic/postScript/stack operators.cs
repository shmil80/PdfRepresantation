using System.Collections.Generic;
using System.Linq;

namespace PdfRepresantation.postScript
{
    class PopOperator : Operator
    {
        public override void Apply(Stack<Operand> stack)
        {
            stack.Pop();
        }
    }

    class ExchOperator : Operator
    {
        public override void Apply(Stack<Operand> stack)
        {
            var operand1 = stack.Pop();
            var operand2 = stack.Pop();
            stack.Push(operand1);
            stack.Push(operand2);
        }
    }

    class DupOperator : Operator
    {
        public override void Apply(Stack<Operand> stack)
        {
            var operand = stack.Peek();
            stack.Push(operand);
        }
    }

    class RollOperator : Operator
    {
        public override void Apply(Stack<Operand> stack)
        {
            var j = (NumberOperand) stack.Pop();
            var n = (NumberOperand) stack.Pop();
            var list = new LinkedList<Operand>();
            for (var i = 0; i < n.IntValue; i++)
            {
                list.AddLast(stack.Pop());
            }

            if (j.IntValue > 0)
                for (var i = 0; i < j.IntValue; i++)
                {
                    list.AddLast(list.First.Value);
                    list.RemoveFirst();
                }
            else
                for (var i = 0; i < j.IntValue; i++)
                {
                    list.AddLast(list.Last.Value);
                    list.RemoveLast();
                }
            if (list.Count==0)
            {
                return;
            }

            var current = list.Last;
            do
            {
                stack.Push(current.Value);
                current = current.Previous;
            } while (current != null);

        }
    }

    class IndexOperator : Operator
    {
        public override void Apply(Stack<Operand> stack)
        {
            var n = (NumberOperand) stack.Pop();
            var element = stack.ElementAt(n.IntValue);
            stack.Push(element);
        }
    }

    class CopyOperator : Operator
    {
        public override void Apply(Stack<Operand> stack)
        {
            var count = (NumberOperand) stack.Pop();
            var toCopy = new Operand[count.IntValue];
            for (var i = 0; i < toCopy.Length; i++)
            {
                toCopy[i] = stack.Pop();
            }

            for (var i = toCopy.Length - 1; i >= 0; i--)
            {
                stack.Push(toCopy[i]);
            }
            for (var i = toCopy.Length - 1; i >= 0; i--)
            {
                stack.Push(toCopy[i]);
            }
        }
    }
}