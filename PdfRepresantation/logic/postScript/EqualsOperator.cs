using System;

namespace PdfRepresantation.postScript
{
    class EqualsOperator : Operator2To1<ValueOperand>
    {
        protected ValueOperand Apply(NumberOperand operand1, NumberOperand operand2)
        {
            if (operand1.IsInt && operand2.IsInt)
                return operand1.IntValue == operand2.IntValue;
            return Math.Abs(operand1.FloatValue - operand2.FloatValue) < 0.000001;
        }

        protected override ValueOperand Apply(ValueOperand operand1, ValueOperand operand2)
        {
            if (operand1 is NumberOperand num1)
                if (operand2 is NumberOperand num2)
                    return Apply(num1, num2);
                else
                    return false;
            if (operand1 is BoolOperand b1)
                if (operand2 is BoolOperand b2)
                    return b1.Value == b2.Value;
                else
                    return false;
            return false;
        }
    }
    class NotEqualsOperator : EqualsOperator
    {
        protected override ValueOperand Apply(ValueOperand operand1, ValueOperand operand2)
        {
            var baseResult = base.Apply(operand1, operand2);
            return !((BoolOperand) baseResult).Value;
        }
    }
}