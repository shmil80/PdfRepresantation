using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PdfRepresantation.postScript;

namespace PdfRepresantation.Test
{
    [TestClass]
    public class PostScriptTest
    {
        [TestMethod]
        public void TestCode()
        {
            var code = new PostScriptParser().Parse(@"
{ 
    2 copy add
    2 div 
    3 1 roll mul
    sqrt
}");
            var args = new[] {15F, 7F}
                .Select(f => (ValueOperand) new NumberOperand(f))
                .ToArray();
            var result = code.Execute(args);
            var nums = result
                .Cast<NumberOperand>()
                .Select(n => n.FloatValue).ToArray();
            if (nums.Length != 2)
                Assert.Fail();
            if (Math.Abs(nums[1] - 11) > 0.0000001 || Math.Abs(nums[0] - 10.2469511) > 0.00001)
                Assert.Fail();
        }
    }
}