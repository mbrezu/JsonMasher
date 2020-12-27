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
            return ParseFilter(state);
        }

        private IJsonMasherOperator ParseFilter(State state)
        {
            var term1 = ParsePipeTerm(state);
            if (state.Current == Tokens.Pipe)
            {
                state.Advance();
                var term2 = ParseFilter(state);
                return Compose.AllParams(term1, term2);
            }
            else
            {
                return term1;
            }
        }

        private IJsonMasherOperator ParsePipeTerm(State state)
        {
            return ParseArithLowerTerm(state);
        }

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
            if (state.Current is Identifier id)
            {
                return ChainSelectors(state, ParseStringSelector(state, id.Id));
            }
            else if (state.Current == Tokens.OpenSquareParen)
            {
                return ChainSelectors(state, ParseSelector(state));
            }
            else
            {
                return Identity.Instance;
            }
        }

        private IJsonMasherOperator ChainSelectors(State state, IJsonMasherOperator first)
        {
            if (state.Current == Tokens.Dot)
            {
                return Compose.AllParams(first, ParseDot(state));
            }
            if (state.Current == Tokens.OpenSquareParen)
            {
                return Compose.AllParams(first, ParseSelector(state));
            }
            else
            {
                return first;
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

        private IJsonMasherOperator ParseStringSelector(State state, string id)
        {
            state.Advance();
            return new StringSelector { Key = id };
        }
    }
}
