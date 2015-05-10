using NUnit.Framework;
using System;

namespace Calculator.Tests
{
	[TestFixture ()]
	public class Test
	{
		Calculator calc = new Calculator ();

		[TestFixtureSetUp ()]
		public void Setup ()
		{
			Core.Common.Logging.Enable ();
		}

		[Test ()]
		public void TestCase ()
		{
			Assert.AreEqual (5, (calc.Eval (exprString: "2+3") as NumericOperand)?.Value);
			Assert.AreEqual (10, (calc.Eval (exprString: "2*3+4") as NumericOperand)?.Value);
			Assert.AreEqual (11, (calc.Eval (exprString: "2*3+(4+1)") as NumericOperand)?.Value);
			Assert.AreEqual (10, (calc.Eval (exprString: "2+3+(4+1)") as NumericOperand)?.Value);
			Assert.AreEqual (10, (calc.Eval (exprString: "2+2*(1+3)") as NumericOperand)?.Value);

			Assert.AreEqual (8, (calc.Eval (exprString: "2^3") as NumericOperand)?.Value);
			Assert.AreEqual (5, (calc.Eval (exprString: "2^3 - 3") as NumericOperand)?.Value);
			Assert.AreEqual (6, (calc.Eval (exprString: "2^3 - 3 + 1") as NumericOperand)?.Value);
			Assert.AreEqual (6, (calc.Eval (exprString: "3 * 10 / 5 ") as NumericOperand)?.Value);
			Assert.AreEqual (12, (calc.Eval (exprString: "(2^3 - 3 + 1) + (3 * 10 / 5)") as NumericOperand)?.Value);
			Assert.AreEqual (12, (calc.Eval (exprString: "2^3 - 3 + 1 + 3 * 10 / 5 ") as NumericOperand)?.Value);

			Assert.AreEqual (7, (calc.Eval (exprString: "2^3 - 3 + 1 + 3 * 10 / 5 - 5") as NumericOperand)?.Value);

			Assert.AreEqual (7, (calc.Eval (exprString: "2^3 - 3 + 1 + 3 * ((4+4*4)/2) / 5 + -5") as NumericOperand)?.Value);
		}

		[TestFixtureTearDown ()]
		public void TearDown ()
		{
			Core.Common.Logging.Finish ();
		}
	}
}

