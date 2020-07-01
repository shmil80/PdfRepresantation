using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PdfRepresantation.postScript
{
    public class PostScriptParser
    {
        Regex parenthesSeparator = new Regex(@"(\{)(\S)|(\S)(\})");
        Regex spaces = new Regex(@"\s+");

        public CodeExecution Parse(string text)
        {
            text = parenthesSeparator.Replace(text, m => m.Captures[1].Value + " " + m.Captures[2].Value);
            var words = spaces.Split(text.Trim());
            CodeExecution block = null;
            Stack<CodeExecution> blocks = new Stack<CodeExecution>();
            foreach (var word in words)
            {
                switch (word)
                {
                    case "":
                        continue;
                    case "{":
                        block = new CodeExecution();
                        blocks.Push(block);
                        break;
                    case "}":
                        var finished = blocks.Pop();
                        if (blocks.Count > 0)
                        {
                            block = blocks.Peek();
                            block.Code.Add(finished);
                        }

                        break;
                    default:
                        block.Code.Add(ParseWord(word));
                        break;
                }
            }

            return block;
        }

        private Operator ParseWord(string word)
        {
            switch (word)
            {
                case "copy": return new CopyOperator();
                case "add": return new AddOperator();
                case "and": return new AndOperator();
                case "atan": return new AtanOperator();
                case "bitsshift": return new BitsShiftOperator();
                case "true": return new BoolOperand(true);
                case "false": return new BoolOperand(false);
                case "ceiling": return new CeilingOperator();
                case "abs": return new AbsOperator();
                case "cos": return new CosOperator();
                case "div": return new DivOperator();
                case "dup": return new DupOperator();
                case "ne": return new NotEqualsOperator();
                case "equals": return new EqualsOperator();
                case "exch": return new ExchOperator();
                case "exp": return new ExpOperator();
                case "floor": return new FloorOperator();
                case "ge": return new GeOperator();
                case "gt": return new GtOperator();
                case "idiv": return new IdivOperator();
                case "ifelse": return new IfElseOperator();
                case "if": return new IfOperator();
                case "index": return new IndexOperator();
                case "le": return new LeOperator();
                case "ln": return new LnOperator();
                case "log": return new LogOperator();
                case "lt": return new LtOperator();
                case "mod": return new ModOperator();
                case "neg": return new NegOperator();
                case "not": return new NotOperator();
                case "or": return new OrOperator();
                case "pop": return new PopOperator();
                case "roll": return new RollOperator();
                case "round": return new RoundOperator();
                case "sin": return new SinOperator();
                case "sqrt": return new SqrtOperator();
                case "sub": return new SubOperator();
                 case "mul": return new MulOperator();
                case "turncate": return new TurncateOperator();
                case "xor": return new XorOperator();
                default:
                    if (word.Contains("."))
                        return new NumberOperand(float.Parse(word));
                    return new NumberOperand(int.Parse(word));
            }
        }
    }
}