using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32.SafeHandles;

namespace PdfRepresantation.postScript
{
    public abstract class Operator
    {
        public abstract void Apply(Stack<Operand> stack);
    }


   

    abstract class Operator2To1<T> : Operator where T : ValueOperand
    {
        public override void Apply(Stack<Operand> stack)
        {
            var operand2 = stack.Pop();
            var operand1 = stack.Pop();
            var result = Apply((T) operand1, (T) operand2);
            stack.Push(result);
        }

        protected abstract ValueOperand Apply(T operand1, T operand2);
    }

    abstract class Operator1To1<T> : Operator where T : ValueOperand
    {
        public override void Apply(Stack<Operand> stack)
        {
            var operand = stack.Pop();
            var result = Apply((T) operand);
            stack.Push(result);
        }

        protected abstract ValueOperand Apply(T operand);
    }

    
}