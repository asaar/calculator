using System;
using System.Linq;
using System.Collections.Generic;
using Core.Common;

namespace Calculator
{
	public class Calculator
	{
		public Operand Eval (string exprString)
		{
			try {
				Parser parser = new Parser () {
					UnaryOperators = unaryOperators,
					BinaryOperators = binaryOperators,
					Characters = characters
				};
				Operand result = parser.Parse (exprString);
				Log.Info ("result: ", result);
				return result;
			} catch (Exception ex) {
				Log.Error (ex);
				return null;
			}
		}

		private readonly Operator[] unaryOperators = new Operator[] {
			new UnaryOperator (operatorChar: "+", implNumeric: o => o),
			new UnaryOperator (operatorChar: "-", implNumeric: o => o.Negative),
			new UnaryOperator (operatorChar: "!", implBoolean: o => o.Not),
		};

		private readonly Operator[][] binaryOperators = new Operator[][] {
			new [] {
				new BinaryOperator (operatorChar: "^", implNumeric: (l, r) => l.Power (r)),
				new BinaryOperator (operatorChar: "**", implNumeric: (l, r) => l.Power (r)),
			},
			new [] {
				new BinaryOperator (operatorChar: "*", implNumeric: (l, r) => l.Multiply (r)),
				new BinaryOperator (operatorChar: "/", implNumeric: (l, r) => l.Multiply (r.Reciprocal)),
			},
			new [] {
				new BinaryOperator (operatorChar: "%", implNumeric: (l, r) => l.Modulo (r)),
			},
			new [] {
				new BinaryOperator (operatorChar: "-", implNumeric: (l, r) => l.Add (r.Negative)),
				new BinaryOperator (operatorChar: "+-", implNumeric: (l, r) => l.Add (r.Negative)),
			},
			new [] {
				new BinaryOperator (operatorChar: "+", implNumeric: (l, r) => l.Add (r)),
			},
			new [] {
				new BinaryOperator (operatorChar: "==", implBoolean: (l, r) => l.Equality (r)),
				new BinaryOperator (operatorChar: "!=", implBoolean: (l, r) => l.Equality (r).Not),
			},
			new [] { new BinaryOperator (operatorChar: "&&", implBoolean: (l, r) => l.And (r)), },
			new [] { new BinaryOperator (operatorChar: "||", implBoolean: (l, r) => l.Or (r)), },
		};

		private readonly Dictionary<char, TokenType> characters = new Dictionary<char, TokenType> {
			['0' ] = TokenType.VALUE,
			['1' ] = TokenType.VALUE,
			['2' ] = TokenType.VALUE,
			['3' ] = TokenType.VALUE,
			['4' ] = TokenType.VALUE,
			['5' ] = TokenType.VALUE,
			['6' ] = TokenType.VALUE,
			['7' ] = TokenType.VALUE,
			['8' ] = TokenType.VALUE,
			['9' ] = TokenType.VALUE,
			['.' ] = TokenType.VALUE,
			['+' ] = TokenType.OPERATOR,
			['-' ] = TokenType.OPERATOR,
			['!' ] = TokenType.OPERATOR,
			['^' ] = TokenType.OPERATOR,
			['*' ] = TokenType.OPERATOR,
			['/' ] = TokenType.OPERATOR,
			['%' ] = TokenType.OPERATOR,
			['=' ] = TokenType.OPERATOR,
			['&' ] = TokenType.OPERATOR,
			['|' ] = TokenType.OPERATOR,
			['(' ] = TokenType.OPENING_BRACKET,
			[')' ] = TokenType.CLOSING_BRACKET,
		};
	}
}

