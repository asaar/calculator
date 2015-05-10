using System;
using System.Linq;
using System.Collections.Generic;
using Core.Common;

namespace Calculator
{

	public class Parser
	{
		public Operator[] UnaryOperators { get; set; } = new Operator[0];

		public Operator[][] BinaryOperators { get; set; } = new Operator[][] {};

		public Dictionary<char, TokenType> Characters { get; set; } = new Dictionary<char, TokenType> ();

		public Operand Result { get; private set; } = Operand.None;

		public Operand Parse (string exprString)
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

			return Result = ParseTerm (tokens: ref tokens);
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
				maxBinaryGrammarRule = BinaryOperators.Length - 1;
			}
			if (tokens.Count == 0) {
				return Operand.None;
			}
			Token first = tokens.First ();

			Log.Debug ("ParseTerm: (1) tokens: ", string.Join (" ", tokens.Select (t => t.Text)));
			Operand left = ParseValue (tokens: ref tokens);
			Log.Debug ("ParseTerm: (2) tokens: ", string.Join (" ", tokens.Select (t => t.Text)));
			if (left == Operand.None) {
				left = ParseUnaryOperator (tokens: ref tokens);
			}
			Log.Debug ("ParseTerm: (3) tokens: ", string.Join (" ", tokens.Select (t => t.Text)));
			if (left == Operand.None) {
				Log.Warning ("Unknown token: ", first);
			}

			if (left != Operand.None) {
				Log.Indent++;
				left = ParseBinaryOperator (left: left, tokens: ref tokens, grammarRule: maxBinaryGrammarRule);
				Log.Indent--;
			}
			Log.Debug ("ParseTerm: (4) tokens: ", string.Join (" ", tokens.Select (t => t.Text)));

			return left;
		}

		private Operand ParseBinaryOperator (Operand left, ref List<Token> tokens, int grammarRule = int.MaxValue)
		{
			if (grammarRule == int.MaxValue) {
				grammarRule = BinaryOperators.Length - 1;
			}
			if (tokens.Count == 0) {
				return left;
			}
			Token first = tokens.First ();

			if (grammarRule < 0)
				return ParseTerm (tokens: ref tokens);
			Operator[] operators = BinaryOperators [grammarRule];
			Log.Debug ("ParseBinaryOperator: grammarRule: ", grammarRule, ", operators: ", operators.Join (","));

			if (first.Type == TokenType.OPERATOR) {
				while (tokens.Count > 0) {
					first = tokens.First ();
					Log.Debug ("tokens: ", string.Join (" ", tokens.Select (t => t.Text)));
					if (first.Type == TokenType.OPERATOR) {
						// if the current operator is this binary operator 
						if (operators.Any (o => first.Text == o.OperatorChar)) {
							Operator op = operators.First (o => first.Text == o.OperatorChar);
							Log.Debug ("found binary operator: ", op);
							tokens.RemoveAt (0);
							Log.Indent++;
							Operand right = ParseTerm (tokens: ref tokens, maxBinaryGrammarRule: grammarRule);
							Log.Indent--;
							left = (op as BinaryOperator).Eval (l: left, r: right);
						}
						// if the current operator has higher precedence as this one
						else if (BinaryOperators.Take (grammarRule).Any (ops => ops.Any (o => o.OperatorChar == first.Text))) {
							Log.Indent++;
							left = ParseBinaryOperator (left: left, tokens: ref tokens, grammarRule: grammarRule - 1);
							Log.Indent--;
						}
						// if the current operator has lower precedence as this one
						else {
							break;
						}
					} else if (first.Type == TokenType.CLOSING_BRACKET) {
						break; // ignore
					} else {
						Log.Debug ("ParseBinaryOperator: Invalid token (1): should be binary operator: " + first);
					}
				}
			} else if (first.Type == TokenType.CLOSING_BRACKET) {
				// ignore
			} else {
				Log.Debug ("ParseBinaryOperator: Invalid token (2): should be binary operator: " + first);
			}

			return left;
		}

		private Operand ParseUnaryOperator (ref List<Token> tokens, int grammarRule = int.MaxValue)
		{
			if (grammarRule == int.MaxValue) {
				grammarRule = UnaryOperators.Length - 1;
			}
			if (tokens.Count == 0) {
				return Operand.None;
			}
			Token first = tokens.First ();

			if (grammarRule < 0)
				return ParseTerm (tokens: ref tokens);
			Operator op = UnaryOperators [grammarRule];
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
}
