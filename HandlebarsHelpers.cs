using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandlebarsDotNet;

namespace Handlebars
{
    public class AdditionalHelpers
    {
        public class ContidionalToken
        {
            public enum TokenType
            {
                OpenParenthese,
                CloseParenthese,
                UnaryOperation,
                BinaryOperation,
                Literal,
                ExpressionEnd
            }

            public enum TokenOperator
            {
                OpenParenthese,
                CloseParenthese,
                Not,
                And,
                Or,
                Equal,
                NotEqual,
                LessThan,
                LessThanOrEqual,
                GreaterThan,
                GreaterThanOrEqual,
                NoOperator
            }

            static Dictionary<string, KeyValuePair<TokenType, TokenOperator>> tokens = new Dictionary<string, KeyValuePair<TokenType, TokenOperator>>()
            {
                { "(", new KeyValuePair<TokenType, TokenOperator>(TokenType.OpenParenthese     , TokenOperator.OpenParenthese) },
                { ")", new KeyValuePair<TokenType, TokenOperator>(TokenType.CloseParenthese    , TokenOperator.CloseParenthese) },
                { "NOT", new KeyValuePair<TokenType, TokenOperator>(TokenType.UnaryOperation   , TokenOperator.Not ) },
                { "!", new KeyValuePair<TokenType, TokenOperator>(TokenType.UnaryOperation     , TokenOperator.Not) },
                { "AND", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation  , TokenOperator.And) },
                { "&&", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation   , TokenOperator.And) },
                { "OR", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation   , TokenOperator.Or) },
                { "||", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation   , TokenOperator.Or) },
                { "==", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation   , TokenOperator.Equal) },
                { "EQ", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation   , TokenOperator.Equal) },
                { "!=", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation   , TokenOperator.NotEqual) },
                { "NEQ", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation  , TokenOperator.NotEqual) },
                { "<", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation    , TokenOperator.LessThan) },
                { "LT", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation   , TokenOperator.LessThan) },
                { "<=", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation   , TokenOperator.LessThanOrEqual) },
                { "LEQ", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation  , TokenOperator.LessThanOrEqual) },
                { ">=", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation   , TokenOperator.GreaterThanOrEqual) },
                { "GEQ", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation  , TokenOperator.GreaterThanOrEqual) },
                { ">", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation    , TokenOperator.GreaterThan) },
                { "GT", new KeyValuePair<TokenType, TokenOperator>(TokenType.BinaryOperation   , TokenOperator.GreaterThan) }
            };

            public TokenType Type;
            public TokenOperator Operator;
            public string Value;

            public ContidionalToken(string str)
            {
                str = str.ToUpper();

                if (tokens.ContainsKey(str))
                {
                    Type = tokens[str].Key;
                    Operator = tokens[str].Value;
                    Value = Operator.ToString();
                }
                else
                {
                    Type = TokenType.Literal;
                    Operator = TokenOperator.NoOperator;
                    Value = str;
                }
            }

            public static List<ContidionalToken> ToPolishNotation(List<ContidionalToken> infixTokenList)
            {
                Queue<ContidionalToken> outputQueue = new Queue<ContidionalToken>();
                Stack<ContidionalToken> stack = new Stack<ContidionalToken>();

                int index = 0;
                while (infixTokenList.Count > index)
                {
                    ContidionalToken t = infixTokenList[index];

                    switch (t.Type)
                    {
                        case ContidionalToken.TokenType.Literal:
                            outputQueue.Enqueue(t);
                            break;
                        case ContidionalToken.TokenType.BinaryOperation:
                        case ContidionalToken.TokenType.UnaryOperation:
                        case ContidionalToken.TokenType.OpenParenthese:
                            stack.Push(t);
                            break;
                        case ContidionalToken.TokenType.CloseParenthese:
                            while (stack.Peek().Type != ContidionalToken.TokenType.OpenParenthese)
                            {
                                outputQueue.Enqueue(stack.Pop());
                            }
                            stack.Pop();
                            if (stack.Count > 0 && stack.Peek().Type == ContidionalToken.TokenType.UnaryOperation)
                            {
                                outputQueue.Enqueue(stack.Pop());
                            }
                            break;
                        default:
                            break;
                    }

                    ++index;
                }
                while (stack.Count > 0)
                {
                    outputQueue.Enqueue(stack.Pop());
                }

                return outputQueue.Reverse().ToList();
            }
        }

        public class ConditionalExpression
        {
            //
            //  inner state
            //

            private ContidionalToken.TokenOperator _operator;
            private ConditionalExpression _left;
            private ConditionalExpression _right;
            private string _value;

            //
            //  private constructor
            //

            private ConditionalExpression(ContidionalToken.TokenOperator op, ConditionalExpression left, ConditionalExpression right)
            {
                _operator = op;
                _left = left;
                _right = right;
                _value = null;
            }

            private ConditionalExpression(String literal)
            {
                _operator = ContidionalToken.TokenOperator.NoOperator;
                _left = null;
                _right = null;
                _value = literal;
            }

            //
            //  accessor
            //

            public ContidionalToken.TokenOperator Op
            {
                get { return _operator; }
                set { _operator = value; }
            }

            public ConditionalExpression Left
            {
                get { return _left; }
                set { _left = value; }
            }

            public ConditionalExpression Right
            {
                get { return _right; }
                set { _right = value; }
            }

            public string Value
            {
                get { return _value; }
                set { _value = value; }
            }

            //
            //  public factory
            //

            public static ConditionalExpression CreateAnd(ConditionalExpression left, ConditionalExpression right)
            {
                return new ConditionalExpression(ContidionalToken.TokenOperator.And, left, right);
            }

            public static ConditionalExpression CreateNot(ConditionalExpression child)
            {
                return new ConditionalExpression(ContidionalToken.TokenOperator.Not, child, null);
            }

            public static ConditionalExpression CreateOr(ConditionalExpression left, ConditionalExpression right)
            {
                return new ConditionalExpression(ContidionalToken.TokenOperator.Or, left, right);
            }

            public static ConditionalExpression CreateEq(ConditionalExpression left, ConditionalExpression right)
            {
                return new ConditionalExpression(ContidionalToken.TokenOperator.Equal, left, right);
            }

            public static ConditionalExpression CreateNeq(ConditionalExpression left, ConditionalExpression right)
            {
                return new ConditionalExpression(ContidionalToken.TokenOperator.NotEqual, left, right);
            }

            public static ConditionalExpression CreateLt(ConditionalExpression left, ConditionalExpression right)
            {
                return new ConditionalExpression(ContidionalToken.TokenOperator.LessThan, left, right);
            }

            public static ConditionalExpression CreateLeq(ConditionalExpression left, ConditionalExpression right)
            {
                return new ConditionalExpression(ContidionalToken.TokenOperator.LessThanOrEqual, left, right);
            }

            public static ConditionalExpression CreateGt(ConditionalExpression left, ConditionalExpression right)
            {
                return new ConditionalExpression(ContidionalToken.TokenOperator.GreaterThan, left, right);
            }

            public static ConditionalExpression CreateGeq(ConditionalExpression left, ConditionalExpression right)
            {
                return new ConditionalExpression(ContidionalToken.TokenOperator.GreaterThanOrEqual, left, right);
            }

            public static ConditionalExpression CreateLiteral(String str)
            {
                return new ConditionalExpression(str);
            }

            public static ConditionalExpression Create(ref List<ContidionalToken>.Enumerator polishNotationTokensEnumerator)
            {
                if (polishNotationTokensEnumerator.Current.Type == ContidionalToken.TokenType.Literal)
                {
                    ConditionalExpression literal = ConditionalExpression.CreateLiteral(polishNotationTokensEnumerator.Current.Value);
                    polishNotationTokensEnumerator.MoveNext();
                    return literal;
                }
                else
                {
                    switch (polishNotationTokensEnumerator.Current.Operator)
                    {
                        case ContidionalToken.TokenOperator.Not:
                            {
                                polishNotationTokensEnumerator.MoveNext();
                                ConditionalExpression operand = Create(ref polishNotationTokensEnumerator);
                                return ConditionalExpression.CreateNot(operand);
                            }
                        case ContidionalToken.TokenOperator.And:
                            {
                                polishNotationTokensEnumerator.MoveNext();
                                ConditionalExpression left = Create(ref polishNotationTokensEnumerator);
                                ConditionalExpression right = Create(ref polishNotationTokensEnumerator);
                                return ConditionalExpression.CreateAnd(left, right);
                            }
                        case ContidionalToken.TokenOperator.Or:
                            {
                                polishNotationTokensEnumerator.MoveNext();
                                ConditionalExpression left = Create(ref polishNotationTokensEnumerator);
                                ConditionalExpression right = Create(ref polishNotationTokensEnumerator);
                                return ConditionalExpression.CreateOr(left, right);
                            }
                        case ContidionalToken.TokenOperator.Equal:
                            {
                                polishNotationTokensEnumerator.MoveNext();
                                ConditionalExpression left = Create(ref polishNotationTokensEnumerator);
                                ConditionalExpression right = Create(ref polishNotationTokensEnumerator);
                                return ConditionalExpression.CreateEq(left, right);
                            }
                        case ContidionalToken.TokenOperator.NotEqual:
                            {
                                polishNotationTokensEnumerator.MoveNext();
                                ConditionalExpression left = Create(ref polishNotationTokensEnumerator);
                                ConditionalExpression right = Create(ref polishNotationTokensEnumerator);
                                return ConditionalExpression.CreateNeq(left, right);
                            }
                        case ContidionalToken.TokenOperator.LessThan:
                            {
                                polishNotationTokensEnumerator.MoveNext();
                                ConditionalExpression left = Create(ref polishNotationTokensEnumerator);
                                ConditionalExpression right = Create(ref polishNotationTokensEnumerator);
                                return ConditionalExpression.CreateLt(left, right);
                            }
                        case ContidionalToken.TokenOperator.LessThanOrEqual:
                            {
                                polishNotationTokensEnumerator.MoveNext();
                                ConditionalExpression left = Create(ref polishNotationTokensEnumerator);
                                ConditionalExpression right = Create(ref polishNotationTokensEnumerator);
                                return ConditionalExpression.CreateLeq(left, right);
                            }
                        case ContidionalToken.TokenOperator.GreaterThan:
                            {
                                polishNotationTokensEnumerator.MoveNext();
                                ConditionalExpression left = Create(ref polishNotationTokensEnumerator);
                                ConditionalExpression right = Create(ref polishNotationTokensEnumerator);
                                return ConditionalExpression.CreateGt(left, right);
                            }
                        case ContidionalToken.TokenOperator.GreaterThanOrEqual:
                            {
                                polishNotationTokensEnumerator.MoveNext();
                                ConditionalExpression left = Create(ref polishNotationTokensEnumerator);
                                ConditionalExpression right = Create(ref polishNotationTokensEnumerator);
                                return ConditionalExpression.CreateGeq(left, right);
                            }
                        default:
                            {
                                return null;
                            }
                    }
                }
            }

            public bool Eval()
            {
                if (this.IsLeaf())
                {
                    throw new HandlebarsException("Evaluating Conditional expression over Literal is not allowed");
                }

                switch (this.Op)
                {
                    case ContidionalToken.TokenOperator.Not:
                        return !this.Left.Eval();
                    case ContidionalToken.TokenOperator.Or:
                        return this.Left.Eval() || this.Right.Eval();
                    case ContidionalToken.TokenOperator.And:
                        return this.Left.Eval() && this.Right.Eval();
                    case ContidionalToken.TokenOperator.Equal:
                        //If both are Literals than check equality else it's not allowed
                        if (this.Left.IsLeaf() && this.Right.IsLeaf())
                        {
                            return this.Left.Value == this.Right.Value;
                        }
                        else
                        {
                            throw new HandlebarsException("{{EQ}} must compare two literals");
                        }
                    case ContidionalToken.TokenOperator.NotEqual:
                        if (this.Left.IsLeaf() && this.Right.IsLeaf())
                        {
                            return this.Left.Value != this.Right.Value;
                        }
                        else
                        {
                            throw new HandlebarsException("{{NEQ}} must compare two literals");
                        }
                    case ContidionalToken.TokenOperator.LessThan:
                        if (this.Left.IsLeaf() && this.Right.IsLeaf())
                        {
                            return this.Left.Value.CompareTo(this.Right.Value) < 0;
                        }
                        else
                        {
                            throw new HandlebarsException("{{LT}} must compare two literals");
                        }
                    case ContidionalToken.TokenOperator.LessThanOrEqual:
                        if (this.Left.IsLeaf() && this.Right.IsLeaf())
                        {
                            return this.Left.Value.CompareTo(this.Right.Value) <= 0;
                        }
                        else
                        {
                            throw new HandlebarsException("{{LEQ}} must compare two literals");
                        }
                    case ContidionalToken.TokenOperator.GreaterThan:
                        if (this.Left.IsLeaf() && this.Right.IsLeaf())
                        {
                            return this.Left.Value.CompareTo(this.Right.Value) > 0;
                        }
                        else
                        {
                            throw new HandlebarsException("{{GT}} must compare two literals");
                        }
                    case ContidionalToken.TokenOperator.GreaterThanOrEqual:
                        if (this.Left.IsLeaf() && this.Right.IsLeaf())
                        {
                            return this.Left.Value.CompareTo(this.Right.Value) >= 0;
                        }
                        else
                        {
                            throw new HandlebarsException("{{GEQ}} must compare two literals");
                        }
                }

                throw new ArgumentException();
            }

            public ConditionalExpression(ConditionalExpression other)
            {
                // No share any object on purpose
                _operator = other._operator;
                _left = other._left == null ? null : new ConditionalExpression(other._left);
                _right = other._right == null ? null : new ConditionalExpression(other._right);
                _value = new StringBuilder(other._value).ToString();
            }

            //
            //  state checker
            //

            Boolean IsLeaf()
            {
                return (_operator == ContidionalToken.TokenOperator.NoOperator);
            }

            Boolean IsAtomic()
            {
                return (IsLeaf() || (_operator == ContidionalToken.TokenOperator.Not && _left.IsLeaf()));
            }
        }

        public static void RegisterHelpers()
        {
            Handlebars.RegisterHelper("ifCond", (writer, options, context, arguments) =>
            {
                List<ContidionalToken> tokens = new List<ContidionalToken>();

                foreach (object arg in arguments)
                {
                    tokens.Add(new ContidionalToken(arg.ToString()));
                }

                var polishNotationTokens = ContidionalToken.ToPolishNotation(tokens);


                var enumerator = polishNotationTokens.GetEnumerator();
                enumerator.MoveNext();

                ConditionalExpression expression = ConditionalExpression.Create(ref enumerator);

                if (expression.Eval())
                {
                    options.Template(writer, context);
                }
                else
                {
                    options.Inverse(writer, context);
                }
            });
        }
    }
}
