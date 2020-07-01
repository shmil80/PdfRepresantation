using System;
using System.Collections.Generic;

namespace PdfRepresantation.postScript
{


    class AbsOperator : Operator1To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand)
        {
            if(operand.IsInt)
                return Math.Abs(operand.IntValue);
            return Math.Abs(operand.FloatValue);
        }
    }

  

    class CeilingOperator : Operator1To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand)
        {
            return Math.Ceiling(operand.FloatValue);
        }
    }

    class FloorOperator : Operator1To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand)
        {
            return Math.Floor(operand.FloatValue);
        }
    }

    class RoundOperator : Operator1To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand)
        {
            return Math.Round(operand.FloatValue);
        }
    }

    class NegOperator : Operator1To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand)
        {
            if(operand.IsInt)
                return -operand.IntValue;
            return -operand.FloatValue;
        }
    }

    class TurncateOperator : Operator1To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand)
        {
            if (operand.IsInt)
                return operand;
            return (float)(int) operand.FloatValue;
        }
    }

    class ExpOperator : Operator1To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand)
        {
            return Math.Exp(operand.FloatValue);
        }
    }

    class SqrtOperator : Operator1To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand)
        {
            return Math.Sqrt(operand.FloatValue);
        }
    }

    class SinOperator : Operator1To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand)
        {
            return Math.Sin(operand.FloatValue/180*Math.PI);
        }
    }
    class CosOperator : Operator1To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand)
        {
            return Math.Cos(operand.FloatValue/180*Math.PI);
        }
    }
    class LogOperator :  Operator1To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand)
        {
            return Math.Log10(operand.FloatValue);
        }
    }
    class LnOperator : Operator1To1<NumberOperand>
    {
        protected override ValueOperand Apply(NumberOperand operand)
        {
            return Math.Log(operand.FloatValue);
        }
    }
}