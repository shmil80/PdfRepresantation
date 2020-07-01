using System.Collections.Generic;

namespace PdfRepresantation.postScript
{
    public abstract class Operand : Operator
    {
        public override void Apply(Stack<Operand> stack)
        {
            stack.Push(this);
        }
    }

    public abstract class ValueOperand:Operand
    {
        public static implicit operator ValueOperand(float b)=>new NumberOperand(b);  
        public static implicit operator ValueOperand(double b)=>new NumberOperand(b);  
        public static implicit operator ValueOperand(int b)=>new NumberOperand(b);  
        public static implicit operator ValueOperand(bool b)=>new BoolOperand(b);
        
    }
    public  class NumberOperand : ValueOperand
    {
        private readonly float? floatValue;
        private readonly int? intValue;
        public float FloatValue => floatValue ?? intValue.Value;
        public int IntValue => intValue ?? (int)floatValue.Value;
        public bool IsInt => intValue.HasValue;
        public NumberOperand(float value) => floatValue = value;
        public NumberOperand(double value) => floatValue = (float) value;
        public NumberOperand(int value) => intValue =  value;
        public override string ToString() => FloatValue.ToString();
    }

    public class BoolOperand : ValueOperand
    {
        public readonly bool Value;
        public BoolOperand(bool value) => this.Value = value;
        public static explicit operator BoolOperand(NumberOperand source) =>
            new BoolOperand(source.IsInt ? source.IntValue != 0 : source.FloatValue != 0);
        public override string ToString() => Value.ToString();
    }
}