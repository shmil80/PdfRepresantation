namespace PdfRepresantation.postScript
{
    class LeOperator : Operator2To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand1, NumberOperand operand2)
        {
            if (operand1.IsInt && operand2.IsInt)
                return operand1.IntValue <= operand2.IntValue;
            return operand1.FloatValue <= operand2.FloatValue;
        }
    }
    class LtOperator : Operator2To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand1, NumberOperand operand2)
        {
            if (operand1.IsInt && operand2.IsInt)
                return operand1.IntValue < operand2.IntValue;
            return operand1.FloatValue < operand2.FloatValue;
        }
    }

    class GeOperator : Operator2To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand1, NumberOperand operand2)
        {
            if (operand1.IsInt && operand2.IsInt)
                return operand1.IntValue >= operand2.IntValue;
            return operand1.FloatValue >= operand2.FloatValue;
        }
    }

    class GtOperator : Operator2To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand1, NumberOperand operand2)
        {
            if (operand1.IsInt && operand2.IsInt)
                return operand1.IntValue > operand2.IntValue;
            return operand1.FloatValue > operand2.FloatValue;
        }
    }
}