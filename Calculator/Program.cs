using System;

namespace Calculator
{
	public class MainClass
	{
		public static void Main (string[] args)
		{
			Core.IO.Logging.Enable ();

			Console.WriteLine ("Hello World!");
			Calculator calc = new Calculator ();

			//calc.Eval (exprString: "2+3");

			//calc.Eval (exprString: "2*3+4");
			//calc.Eval (exprString: "2*3+(4+1)");
			//calc.Eval (exprString: "2+3+(4+1)");
			//calc.Eval (exprString: "2+3*(2)");
			calc.Eval (exprString: "2+2*(1+3)");

			//calc.Eval (exprString: "10  +(-2*3*(-1))");
			// "2^3 - 3 + 1 + 3 * ((4+4*4)/2) / 5 + -5"

			Core.IO.Logging.Finish ();
		}
	}
}
