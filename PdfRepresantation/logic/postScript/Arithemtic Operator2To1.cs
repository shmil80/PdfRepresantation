using System;
using System.Collections.Generic;

namespace PdfRepresantation.postScript
{
    class DivOperator : Operator2To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand1, NumberOperand operand2)
        {
            return operand1.FloatValue / operand2.FloatValue;
        }
    }

    class IdivOperator : Operator2To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand1, NumberOperand operand2)
        {
            return operand1.IntValue / operand2.IntValue;
        }
    }

    class ModOperator : Operator2To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand1, NumberOperand operand2)
        {
            return operand1.IntValue % operand2.IntValue;
        }
    }

    class AddOperator : Operator2To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand1, NumberOperand operand2)
        {
            if (operand1.IsInt && operand2.IsInt)
                return operand1.IntValue + operand2.IntValue;
            return operand1.FloatValue + operand2.FloatValue;
        }
    }

    class SubOperator : Operator2To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand1, NumberOperand operand2)
        {
            if (operand1.IsInt && operand2.IsInt)
                return operand1.IntValue - operand2.IntValue;
            return operand1.FloatValue - operand2.FloatValue;
        }
    }
    class MulOperator : Operator2To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand1, NumberOperand operand2)
        {
            if (operand1.IsInt && operand2.IsInt)
                return operand1.IntValue * operand2.IntValue;
            return operand1.FloatValue * operand2.FloatValue;
        }
    }

    class BitsShiftOperator : Operator2To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand1, NumberOperand operand2)
        {
            return operand2.IntValue > 0
                ? operand1.IntValue << operand2.IntValue
                : operand1.IntValue >> -operand2.IntValue;
        }
    }

    class AtanOperator : Operator2To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand1, NumberOperand operand2)
        {
            return 180 / Math.PI * Math.Atan2(operand2.FloatValue, operand1.FloatValue);
        }
    }

   

    
}