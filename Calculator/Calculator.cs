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
				Expression expr = new Expression (exprString);
				Log.Info ("result: ", expr.Result);
				return expr.Result;
			} catch (Exception ex) {
				Log.Error (ex);
				return null;
			}
		}
	}

	public class Expression
	{
		private Operator[] UnaryGrammar = new Operator[] {
			new UnaryOperator (operatorChar: "+", implNumeric: o => o),
			new UnaryOperator (operatorChar: "-", implNumeric: o => o.Negative),
			new UnaryOperator (operatorChar: "!", implBoolean: o => o.Not),
		};

		private Operator[] BinaryGrammar = new Operator[] {
			new BinaryOperator (operatorChar: "^", implNumeric: (l, r) => l.Power (r)),
			new BinaryOperator (operatorChar: "*", implNumeric: (l, r) => l.Multiply (r)),
			new BinaryOperator (operatorChar: "/", implNumeric: (l, r) => l.Multiply (r.Reciprocal)),
			//new BinaryOperator (operatorChar: "%"),
			new BinaryOperator (operatorChar: "-", implNumeric: (l, r) => l.Add (r.Negative)),
			new BinaryOperator (operatorChar: "+", implNumeric: (l, r) => l.Add (r)),
			new BinaryOperator (operatorChar: "==", implBoolean: (l, r) => l.Equality (r)),
			new BinaryOperator (operatorChar: "!=", implBoolean: (l, r) => l.Equality (r).Not),
			new BinaryOperator (operatorChar: "&&", implBoolean: (l, r) => l.And (r)),
			new BinaryOperator (operatorChar: "||", implBoolean: (l, r) => l.Or (r)),
		};

		private Dictionary<char, TokenType> Characters = new Dictionary<char, TokenType> {
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
			['=' ] = TokenType.OPERATOR,
			['&' ] = TokenType.OPERATOR,
			['|' ] = TokenType.OPERATOR,
			['(' ] = TokenType.OPENING_BRACKET,
			[')' ] = TokenType.CLOSING_BRACKET,
		};

		public Operand Result { get; private set; } = Operand.None;

		public Expression (string exprString)
		{
			exprString = exprString.Replace ("(-", "(0-");
			exprString = string.Join ("", exprString.Where (c => !char.IsWhiteSpace (c)));

			List<Token> tokens = Tokenize (exprString).Where (s => s.Text.Length > 0).ToList ();
			Log.Info ("Tokens: ");
			Log.Indent++;
			foreach (Token t in tokens) {
				if (t.Type == TokenType.CLOSING_BRACKET)
					Log.Indent--;
				Log.Info (t);
				if (t.Type == TokenType.OPENING_BRACKET && t.Text == "(")
					Log.Indent++;
			}
			Log.Indent--;

			Result = ParseTerm (tokens: ref tokens);
		}

		private IEnumerable<Token> Tokenize (string expr)
		{
			string currentToken = "";
			TokenType currentTokenType = TokenType.INVALID;
			foreach (char c in expr.ToCharArray()) {
				if (Characters.ContainsKey (c)) {
					if (Characters [c] == currentTokenType && currentTokenType != TokenType.OPENING_BRACKET && currentTokenType != TokenType.CLOSING_BRACKET)
						currentToken += c;
					else {
						yield return new Token { Text = currentToken, Type = currentTokenType };
						currentTokenType = Characters [c];
						currentToken = "" + c;
					}
				}
			}
			yield return new Token { Text = currentToken, Type = currentTokenType };
		}

		private Operand ParseTerm (ref List<Token> tokens, int maxBinaryGrammarRule = int.MaxValue)
		{
			if (maxBinaryGrammarRule == int.MaxValue) {
				maxBinaryGrammarRule = BinaryGrammar.Length - 1;
			}
			if (tokens.Count == 0) {
				return Operand.None;
			}
			Token first = tokens.First ();

			Operand left = ParseValue (tokens: ref tokens);
			if (left == Operand.None) {
				left = ParseUnaryOperator (tokens: ref tokens);
			}
			if (left == Operand.None) {
				Log.Warning ("Unknown token: ", first);
			}

			left = ParseBinaryOperator (left: left, tokens: ref tokens, grammarRule: maxBinaryGrammarRule);

			return left;

			/*else {
				throw new ArgumentException ("Invalid token: should be operator: " + first);
				}*/


			/*if (c == '/') { // division
					eatChar ();
					v /= ParseTerm (grammarRule: grammarRule - 1);
				} else if (c == '*' || c == '(') { // multiplication
					if (c == '*')
						eatChar ();
					v *= ParseTerm (grammarRule: grammarRule - 1);
				} else {
					return v;
			}*/
		}

		private Operand ParseBinaryOperator (Operand left, ref List<Token> tokens, int grammarRule = int.MaxValue)
		{
			if (grammarRule == int.MaxValue) {
				grammarRule = BinaryGrammar.Length - 1;
			}
			if (tokens.Count == 0) {
				return left;
			}
			Token first = tokens.First ();

			if (grammarRule < 0)
				return ParseTerm (tokens: ref tokens);
			Operator op = BinaryGrammar [grammarRule];
			Log.Debug ("ParseBinaryOperator: grammarRule: ", grammarRule, ", operator: ", op);

			if (first.Type == TokenType.OPERATOR) {
				while (tokens.Count > 0) {
					first = tokens.First ();
					Log.Debug ("tokens: ", string.Join (", ", tokens.Select (t => t.Text)));
					if (first.Type == TokenType.OPERATOR) {
						// if the current operator is this binary operator 
						if (first.Text == op.OperatorChar) {
							Log.Debug ("found binary operator: ", op);
							tokens.RemoveAt (0);
							Operand right = ParseTerm (tokens: ref tokens, maxBinaryGrammarRule: grammarRule);
							left = (op as BinaryOperator).Eval (l: left, r: right);
						}
						// if the current operator has higher precedence as this one
						else if (BinaryGrammar.Take (grammarRule).Any (rule => rule.OperatorChar == first.Text)) {
							Log.Indent++;
							left = ParseBinaryOperator (left: left, tokens: ref tokens, grammarRule: grammarRule - 1);
							Log.Indent--;
						}
						// if the current operator has lower precedence as this one
						else {
							break;
						}
					} else if (first.Type == TokenType.CLOSING_BRACKET) {
						break;
					} else {
						Log.Debug ("ParseBinaryOperator: Invalid token (1): should be binary operator: " + first);
					}
				}
			} else if (first.Type == TokenType.CLOSING_BRACKET) {
			} else {
				Log.Debug ("ParseBinaryOperator: Invalid token (2): should be binary operator: " + first);
			}

			return left;
		}

		private Operand ParseUnaryOperator (ref List<Token> tokens, int grammarRule = int.MaxValue)
		{
			if (grammarRule == int.MaxValue) {
				grammarRule = UnaryGrammar.Length - 1;
			}
			if (tokens.Count == 0) {
				return Operand.None;
			}
			Token first = tokens.First ();

			if (grammarRule < 0)
				return ParseTerm (tokens: ref tokens);
			Operator op = UnaryGrammar [grammarRule];
			Log.Debug ("ParseUnaryOperator: grammarRule: ", grammarRule, ", operator: ", op);

			if (first.Type == TokenType.OPERATOR) {
				if (first.Text == op.OperatorChar) {
					Log.Debug ("found unary operator: ", op);
					tokens.RemoveAt (0);
					Operand right = ParseTerm (tokens: ref tokens);
					return (op as UnaryOperator).Eval (r: right);
				} else {
					return ParseUnaryOperator (tokens: ref tokens, grammarRule: grammarRule - 1);
				}
			} else {
				Log.Debug ("ParseUnaryOperator: Invalid token: should be unary operator: " + first);
				return Operand.None;
			}
		}

		private Operand ParseValue (ref List<Token> tokens)
		{
			if (tokens.Count == 0) {
				return Operand.None;
			}
			Token first = tokens.First ();

			// opening bracket
			if (first.Type == TokenType.OPENING_BRACKET) {
				tokens.RemoveAt (0);
				Log.Indent++;
				Operand result = ParseTerm (tokens: ref tokens);
				Log.Indent--;
				first = tokens.First ();
				if (first.Type == TokenType.CLOSING_BRACKET) {
					tokens.RemoveAt (0);
				} else {
					throw new ArgumentException ("Invalid token: should be closing bracket: " + first);
				}
				return result;
			}

			// value
			if (first.Type == TokenType.VALUE) {
				tokens.RemoveAt (0);
				Log.Debug ("found value: ", first.Text);
				if (first.Text.ToLower () == "true")
					return Operand.True;
				else if (first.Text.ToLower () == "false")
					return Operand.False;
				else
					return new NumericOperand (first.Text);
			}

			// ??
			//throw new ArgumentException ("Invalid token: should be value: " + first);
			Log.Debug ("Invalid token: should be value: " + first);
			return Operand.None;
		}

		/*public static Expression Parse (ref string exprString, char begin, char end)
		{
			for (int depth = 0, i = 0; i < exprString.Length; ++i) {
				if (exprString [i] == begin)
					depth++;
				if (exprString [i] == end)
					depth--;
			}
		}

		public static Expression Parse (string exprString)
		{
			Expression expr = null;
			if (exprString.StartsWith ("(")) {
				exprString = exprString.Substring (1);
				Expression subExpr = Expression.Parse (exprString: ref exprString, begin: '(', end: ')');

			}
			return expr;
		}*/
	}

	public struct Token
	{
		public string Text;
		public TokenType Type;

		public override string ToString ()
		{
			return Type == TokenType.VALUE ? "'" + Text + "': value"
				: Type == TokenType.OPERATOR ? "'" + Text + "': op"
				: Type == TokenType.OPENING_BRACKET ? "'" + Text + "'"
				: Type == TokenType.CLOSING_BRACKET ? "'" + Text + "'"
				: "INVALID";
		}
	}

	public enum TokenType
	{
		VALUE,
		OPERATOR,
		OPENING_BRACKET,
		CLOSING_BRACKET,
		INVALID
	}

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

		public BooleanOperand Equality (NumericOperand other)
		{
			return other != null && Math.Abs (other.Value - Value) < 0.00001 ? Operand.True : Operand.False;
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

	public static class ExpressionExtensions
	{

	}
}

