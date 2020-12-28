using System;
using System.Collections.Generic;
using System.Linq;
using JsonMasher.Mashers;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Operators;
using JsonMasher.Mashers.Primitives;

namespace JsonMasher.Compiler
{
    public class Parser : IParser
    {
        class State
        {
            private Token[] _tokens;
            private int _index;

            public State(IEnumerable<Token> tokens)
            {
                _tokens = tokens.ToArray();
                _index = 0;
            }

            public bool AtEnd => _index == _tokens.Length;

            public Token Current => _index < _tokens.Length ? _tokens[_index] : null;

            public Token Next => _index >= _tokens.Length - 1 ? null : _tokens[_index + 1];

            public State Advance()
            {
                if (!AtEnd) {
                    _index ++;
                }
                return this;
            }

            internal void Match(Token expected)
            {
                if (Current != expected)
                {
                    throw new NotImplementedException();
                }
                Advance();
            }
        }

        public IJsonMasherOperator Parse(string program)
        {
            if (string.IsNullOrWhiteSpace(program))
            {
                return Identity.Instance;
            }
            var state = new State(new Lexer().Tokenize(program));
            var result = ParseFilter(state);
            if (!state.AtEnd)
            {
                throw new InvalidOperationException();
            }
            return result;
        }

        private IJsonMasherOperator ParseFilter(State state)
            => ChainIntoArray(
                state,
                ParsePipeTerm, 
                token => token == Tokens.Pipe,
                Compose.All);

        private IJsonMasherOperator ChainIntoArray(
            State state, 
            Func<State, IJsonMasherOperator> termParser, 
            Func<Token, bool> isLinkToken,
            Func<IEnumerable<IJsonMasherOperator>, IJsonMasherOperator> combiner)
        {
            var terms = new List<IJsonMasherOperator>();
            terms.Add(termParser(state));
            while (isLinkToken(state.Current))
            {
                state.Advance();
                terms.Add(termParser(state));
            }
            if (terms.Count > 1)
            {
                return combiner(terms);
            }
            else
            {
                return terms[0];
            }
        }

        private IJsonMasherOperator ParsePipeTerm(State state)
            => ChainIntoArray(
                state,
                ParseCommaTerm, 
                token => token == Tokens.Comma,
                Concat.All);

        private IJsonMasherOperator ParseCommaTerm(State state)
        {
            var t1 = ParseRelationalTerm(state);
            if (state.Current == Tokens.PipeEquals) {
                state.Advance();
                var t2 = ParseRelationalTerm(state);
                return new PipeAssignment {
                    PathExpression = t1,
                    Masher = t2
                };
            }
            else
            {
                return t1;
            }
        }

        private IJsonMasherOperator ParseRelationalTerm(State state) 
            => ChainAssocLeft(
                state,
                ParseArithLowerTerm,
                op => op == Tokens.EqualsEquals,
                op => EqualsEquals.Operator);

        private IJsonMasherOperator ParseArithLowerTerm(State state)
        {
            return ChainAssocLeft(
                state,
                ParseArithHigherTerm,
                op => op == Tokens.Plus || op == Tokens.Minus,
                op => op == Tokens.Plus ? Plus.Operator : Minus.Operator);
        }

        private IJsonMasherOperator ChainAssocLeft(
            State state, 
            Func<State, IJsonMasherOperator> termParser,
            Func<Token, bool> validOps,
            Func<Token, Func<Json, Json, Json>> opFunc)
        {
            var accum = termParser(state);
            while (validOps(state.Current))
            {
                var op = state.Current;
                state.Advance();
                accum = new BinaryOperator {
                    First = accum,
                    Second = termParser(state),
                    Operator = opFunc(op)
                };
            }
            return accum;
        }

        private IJsonMasherOperator ParseArithHigherTerm(State state)
        {
            return ChainAssocLeft(
                state,
                ParseTerm,
                op => op == Tokens.Times,
                op => Times.Operator);
        }

        private IJsonMasherOperator ParseTerm(State state)
        {
            if (state.Current == Tokens.Dot)
            {
                return ParseDot(state);
            }
            else if (state.Current is Number n)
            {
                state.Advance();
                return new Literal { Value = Json.Number(n.Value) };
            }
            else if (state.Current is String s)
            {
                state.Advance();
                return new Literal { Value = Json.String(s.Value) };
            }
            else if (state.Current is Identifier identifier)
            {
                state.Advance();
                return identifier.Id switch {
                    "null" => new Literal { Value = Json.Null },
                    "true" => new Literal { Value = Json.True },
                    "false" => new Literal { Value = Json.False },
                    _ => throw new InvalidOperationException()
                };
            }
            else if (state.Current == Tokens.OpenParen)
            {
                state.Advance();
                var result = ParseFilter(state);
                state.Match(Tokens.CloseParen);
                return result;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private IJsonMasherOperator ParseDot(State state)
        {
            state.Advance();
            var terms = new List<IJsonMasherOperator>();
            if (state.Current == Tokens.OpenSquareParen)
            {
                terms.Add(ParseSelector(state));
            }
            else if (state.Current is Identifier identifier)
            {
                state.Advance();
                terms.Add(new StringSelector { Key = identifier.Id });
            }
            else
            {
                terms.Add(Identity.Instance);
            }
            while (state.Current == Tokens.Dot || state.Current == Tokens.OpenSquareParen)
            {
                if (state.Current == Tokens.Dot)
                {
                    terms.Add(ParseDotOrStringSelector(state));
                }
                else if (state.Current == Tokens.OpenSquareParen)
                {
                    terms.Add(ParseSelector(state));
                }
            }
            if (terms.Count > 1)
            {
                return Compose.All(terms);
            }
            else
            {
                return terms[0];
            }
        }

        private IJsonMasherOperator ParseDotOrStringSelector(State state)
        {
            state.Match(Tokens.Dot);
            if (state.Current is Identifier identifier)
            {
                state.Advance();
                return new StringSelector { Key = identifier.Id };
            }
            else
            {
                return Identity.Instance;
            }
        }

        private IJsonMasherOperator ParseSelector(State state)
        {
            state.Advance();
            if (state.Current == Tokens.CloseSquareParen)
            {
                state.Advance();
                return Enumerate.Instance;
            }
            else
            {
                var filter = ParsePipeTerm(state);
                state.Match(Tokens.CloseSquareParen);
                return new Selector { Index = filter };
            }
        }
    }
}
