using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JsonMasher.Compiler
{
    public class Lexer : ILexer
    {
        class State
        {
            private string _program;
            private int _index;
            private int _mark;

            public State(string program)
            {
                _program = program;
                _index = 0;
            }

            public bool AtEnd => _index == _program.Length;

            public char Char => _program[_index];
            public char? NextChar => _index + 1 >= _program.Length ? null : _program[_index + 1];

            public void SkipSpaces()
            {
                while (!AtEnd && char.IsWhiteSpace(_program[_index]))
                {
                    _index++;
                }
            }

            public void Advance(int amount = 1) => _index += amount;

            internal void Mark() => _mark = _index;

            internal string GetFromMark() => _program.Substring(_mark, _index - _mark);
        }

        public IEnumerable<Token> Tokenize(string program)
        {
            var state = new State(program);
            state.SkipSpaces();
            while (!state.AtEnd)
            {
                if (state.Char == '.' && state.NextChar == '.')
                {
                    yield return Tokens.DotDot;
                    state.Advance(2);
                }
                else if (state.Char == '=' && state.NextChar == '=')
                {
                    yield return Tokens.EqualsEquals;
                    state.Advance(2);
                }
                else 
                {
                    switch (state.Char)
                    {
                        case '.':
                            yield return Tokens.Dot;
                            state.Advance();
                            break;
                        case '|':
                            yield return Tokens.Pipe;
                            state.Advance();
                            break;
                        case '(':
                            yield return Tokens.OpenParen;
                            state.Advance();
                            break;
                        case ')':
                            yield return Tokens.CloseParen;
                            state.Advance();
                            break;
                        case '[':
                            yield return Tokens.OpenSquareParen;
                            state.Advance();
                            break;
                        case ']':
                            yield return Tokens.CloseSquareParen;
                            state.Advance();
                            break;
                        case '{':
                            yield return Tokens.OpenBrace;
                            state.Advance();
                            break;
                        case '}':
                            yield return Tokens.CloseBrace;
                            state.Advance();
                            break;
                        case ',':
                            yield return Tokens.Comma;
                            state.Advance();
                            break;
                        case ';':
                            yield return Tokens.Semicolon;
                            state.Advance();
                            break;
                        case ':':
                            yield return Tokens.Colon;
                            state.Advance();
                            break;
                        case '+':
                            yield return Tokens.Plus;
                            state.Advance();
                            break;
                        case '-':
                            yield return Tokens.Minus;
                            state.Advance();
                            break;
                        case '*':
                            yield return Tokens.Times;
                            state.Advance();
                            break;
                        default:
                            yield return ComplexToken(state);
                            break;
                    }
                }
                state.SkipSpaces();
            }
        }

        private Token ComplexToken(State state)
        {
            if (Char.IsDigit(state.Char))
            {
                return Number(state);
            }
            else if (Char.IsLetter(state.Char) || state.Char == '_')
            {
                return Identifier(state);
            }
            else if (state.Char == '\"')
            {
                return String(state);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private Token Number(State state)
        {
            state.Mark();
            bool seenDot = false;
            bool inNumber() => (!seenDot && state.Char == '.') || Char.IsDigit(state.Char);
            while (!state.AtEnd && inNumber())
            {
                if (state.Char == '.')
                {
                    seenDot = true;
                }
                state.Advance();
            }
            return Tokens.Number(double.Parse(state.GetFromMark()));
        }

        private Token Identifier(State state)
        {
            state.Mark();
            while (!state.AtEnd && (state.Char == '_' || Char.IsLetterOrDigit(state.Char)))
            {
                state.Advance();
            }
            var id = state.GetFromMark();
            return id switch {
                "def" => Tokens.Keywords.Def,
                _ => Tokens.Identifier(id)
            };
        }

        private Token String(State state)
        {
            state.Advance();
            state.Mark();
            while(true) {
                if (state.AtEnd) {
                    throw new InvalidOperationException();
                }
                else if (state.Char == '\"')
                {
                    var value = state.GetFromMark();
                    state.Advance();
                    try 
                    {
                        var unescaped = Regex.Unescape(value);
                        return new String(unescaped);
                    }
                    catch (RegexParseException ex)
                    {
                        throw new InvalidOperationException("", ex);
                    }
                }
                else if (state.Char == '\\' && state.NextChar != null)
                {
                    state.Advance(2);
                }
                else if (state.Char == '\\')
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    state.Advance();
                }
            }
        }
    }
}
