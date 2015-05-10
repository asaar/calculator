using System;
using System.Linq;
using System.Collections.Generic;
using Core.Common;

namespace Calculator
{
	public abstract class Operand
	{
		public static Operand None { get; } = null;

		public static NumericOperand One { get; } = new NumericOperand(value: 1.0);

		public static BooleanOperand True { get; } = new BooleanOperand(value: true);

		public static BooleanOperand False { get; } = new BooleanOperand(value: false);
	}

	public class NumericOperand : Operand
	{
		public double Value { get; private set; } = 0;

		public NumericOperand (double value)
		{
			Value = value;
		}

		public NumericOperand (string value)
		{
			double doubleValue;
			if (double.TryParse (s: value, result: out doubleValue)) {
				Value = doubleValue;
				Log.Debug ("constructed: ", this);
			} else {
				throw new ArgumentException ("Invalid numeric value: " + value);
			}
		}

		public NumericOperand Negative { get { return new NumericOperand (value: -Value); } }

		public NumericOperand Reciprocal { get { return new NumericOperand (value: 1.0 / Value); } }

		public NumericOperand Add (NumericOperand other)
		{
			return other == null ? this : new NumericOperand (value: Value + other.Value);
		}

		public NumericOperand Multiply (NumericOperand other)
		{
			return other == null ? this : new NumericOperand (value: Value * other.Value);
		}

		public NumericOperand Power (NumericOperand other)
		{
			return other == null ? this : new NumericOperand (value: Math.Pow (Value, other.Value));
		}

		public NumericOperand Modulo (NumericOperand other)
		{
			return other == null ? this : new NumericOperand (value: Value % other.Value);
		}

		public BooleanOperand Equality (NumericOperand other)
		{
			return other != null && Math.Abs (other.Value - Value) < 0.00001 ? Operand.True : Operand.False;
		}

		public BooleanOperand LessThan (NumericOperand other)
		{
			return other != null && Value < other.Value ? Operand.True : Operand.False;
		}

		public override string ToString ()
		{
			return "NumericOperand(" + Value + ")";
		}
	}

	public class BooleanOperand : Operand
	{
		public bool Value { get; private set; }

		public BooleanOperand (bool value)
		{
			Value = value;
		}

		public Operand Not { get { return new BooleanOperand (value: !Value); } }

		public BooleanOperand And (BooleanOperand other)
		{
			return other == null ? this : new BooleanOperand (value: Value && other.Value);
		}

		public BooleanOperand Or (BooleanOperand other)
		{
			return other == null ? this : new BooleanOperand (value: Value || other.Value);
		}

		public BooleanOperand Equality (BooleanOperand other)
		{
			return other != null && Value == other.Value ? Operand.True : Operand.False;
		}

		public override string ToString ()
		{
			return "BooleanOperand(" + (Value ? "True" : "False") + ")";
		}
	}
}
