namespace PdfRepresantation.postScript
{
    class AndOperator : Operator2To1<BoolOperand>
    {
        protected override ValueOperand Apply(BoolOperand operand1, BoolOperand operand2)
        {
            return new BoolOperand(operand1.Value&&operand2.Value);
        }
    }
    class OrOperator : Operator2To1<BoolOperand>
    {
        protected override ValueOperand Apply(BoolOperand operand1, BoolOperand operand2)
        {
            return new BoolOperand(operand1.Value||operand2.Value);
        }
    }
    class XorOperator : Operator2To1<BoolOperand>
    {
        protected override ValueOperand Apply(BoolOperand operand1, BoolOperand operand2)
        {
            return new BoolOperand(operand1.Value!=operand2.Value);
        }
    }
    class NotOperator : Operator1To1<BoolOperand>
    {
        protected override ValueOperand Apply(BoolOperand operand)
        {
            return new BoolOperand(!operand.Value);
        }
    }
    
}