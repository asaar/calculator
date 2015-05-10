using System;
using System.Linq;
using System.Collections.Generic;
using Core.Common;

namespace Calculator
{
	public abstract class Operator
	{
		public string OperatorChar { get; protected set; }
	}

	public class BinaryOperator : Operator
	{
		private readonly Func<NumericOperand, NumericOperand, Operand> ImplNumeric;
		private readonly Func<BooleanOperand, BooleanOperand, Operand> ImplBoolean;

		public BinaryOperator (string operatorChar, Func<NumericOperand, NumericOperand, Operand> implNumeric = null, Func<BooleanOperand, BooleanOperand, Operand> implBoolean = null)
		{
			OperatorChar = operatorChar;
			ImplNumeric = implNumeric;
			ImplBoolean = implBoolean;
		}

		public Operand Eval (Operand l, Operand r)
		{
			Operand result;
			if (l is NumericOperand && r is NumericOperand && ImplNumeric != null)
				result = ImplNumeric (l as NumericOperand, r as NumericOperand);
			else if (l is BooleanOperand && r is BooleanOperand && ImplBoolean != null)
				result = ImplBoolean (l as BooleanOperand, r as BooleanOperand);
			else
				throw new ArgumentException ("Binary Operator " + OperatorChar + " is not defined for: left: " + l?.GetType () + ", right: " + r?.GetType ());
			Log.Debug ("binary operator: ", OperatorChar, "(", l, ", ", r, ") => ", result);
			return result;
		}

		public override string ToString ()
		{
			return "BinaryOperator(" + OperatorChar + ")";
		}
	}

	public class UnaryOperator : Operator
	{
		private readonly Func<NumericOperand, Operand> ImplNumeric;
		private readonly Func<BooleanOperand, Operand> ImplBoolean;

		public UnaryOperator (string operatorChar, Func<NumericOperand, Operand> implNumeric = null, Func<BooleanOperand, Operand> implBoolean = null)
		{
			OperatorChar = operatorChar;
			ImplNumeric = implNumeric;
			ImplBoolean = implBoolean;
		}

		public Operand Eval (Operand r)
		{
			Operand result;
			if (r is NumericOperand && ImplNumeric != null)
				result = ImplNumeric (r as NumericOperand);
			else if (r is BooleanOperand && ImplBoolean != null)
				result = ImplBoolean (r as BooleanOperand);
			else
				throw new ArgumentException ("Unary Operator " + OperatorChar + " is not defined for: right: " + r.GetType ());
			Log.Debug ("unary operator: ", OperatorChar, "(", r, ") => ", result);
			return result;
		}

		public override string ToString ()
		{
			return "UnaryOperator(" + OperatorChar + ")";
		}
	}
}
