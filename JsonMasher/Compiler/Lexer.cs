using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JsonMasher.Compiler
{
    public class Lexer
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

            public char Current => _program[_index];
            public char? Next => _index + 1 >= _program.Length ? null : _program[_index + 1];

            public void SkipWhiteSpaceAndComments()
            {
                while (!AtEnd && (Current == '#' || Char.IsWhiteSpace(Current)))
                {
                    SkipWhiteSpace();
                    SkipComments();
                }
            }

            private void SkipWhiteSpace()
            {
                while (!AtEnd && char.IsWhiteSpace(Current))
                {
                    Advance();
                }
            }

            private void SkipComments()
            {
                if (!AtEnd && Current == '#')
                {
                    while (!AtEnd && Current != '\n')
                    {
                        Advance();
                    }
                }
            }

            public void Advance(int amount = 1) => _index += amount;

            public void SetMark() => _mark = _index;
            public int Mark => _mark;

            public string GetFromMark() => _program.Substring(_mark, _index - _mark);

            public TokenWithPos TokenWithPos(
                Token dotDot, int length = 1, int? startPosNullable = null)
            {
                var startPos = startPosNullable ?? _index;
                return new TokenWithPos(dotDot, startPos, startPos + length);
            }

            internal Exception Error(
                string message,
                int? positionNullable = null,
                int? lengthNullable = null,
                Exception ex = null)
            {
                var position = positionNullable == null ? _index : positionNullable.Value;
                var length = lengthNullable == null ? 1 : lengthNullable.Value;
                var programWithLines = new ProgramWithLines(_program);
                return new JsonMasherException(
                    message, 
                    programWithLines.GetLineNumber(position) + 1,
                    programWithLines.GetColumnNumber(position) + 1,
                    PositionHighlighter.Highlight(programWithLines, position, position + length),
                    ex);
            }
        }

        public IEnumerable<TokenWithPos> Tokenize(string program)
        {
            var state = new State(program);
            state.SkipWhiteSpaceAndComments();
            while (!state.AtEnd)
            {
                if (state.Current == '.' && state.Next == '.')
                {
                    yield return state.TokenWithPos(Tokens.DotDot, 2);
                    state.Advance(2);
                }
                else if (state.Current == '.' 
                    && state.Next.HasValue 
                    && Char.IsDigit(state.Next.Value))
                {
                    yield return Number(state);
                }
                else if (state.Current == '=' && state.Next == '=')
                {
                    yield return state.TokenWithPos(Tokens.EqualsEquals, 2);
                    state.Advance(2);
                }
                else if (state.Current == '!' && state.Next == '=')
                {
                    yield return state.TokenWithPos(Tokens.NotEquals, 2);
                    state.Advance(2);
                }
                else if (state.Current == '|' && state.Next == '=')
                {
                    yield return state.TokenWithPos(Tokens.PipeEquals, 2);
                    state.Advance(2);
                }
                else if (state.Current == '<' && state.Next == '=')
                {
                    yield return state.TokenWithPos(Tokens.LessThanOrEqual, 2);
                    state.Advance(2);
                }
                else if (state.Current == '>' && state.Next == '=')
                {
                    yield return state.TokenWithPos(Tokens.GreaterThanOrEqual, 2);
                    state.Advance(2);
                }
                else if (state.Current == '/' && state.Next == '/')
                {
                    yield return state.TokenWithPos(Tokens.SlashSlash, 2);
                    state.Advance(2);
                }
                else 
                {
                    switch (state.Current)
                    {
                        case '.':
                            yield return state.TokenWithPos(Tokens.Dot);
                            state.Advance();
                            break;
                        case '|':
                            yield return state.TokenWithPos(Tokens.Pipe);
                            state.Advance();
                            break;
                        case '(':
                            yield return state.TokenWithPos(Tokens.OpenParen);
                            state.Advance();
                            break;
                        case ')':
                            yield return state.TokenWithPos(Tokens.CloseParen);
                            state.Advance();
                            break;
                        case '[':
                            yield return state.TokenWithPos(Tokens.OpenSquareParen);
                            state.Advance();
                            break;
                        case ']':
                            yield return state.TokenWithPos(Tokens.CloseSquareParen);
                            state.Advance();
                            break;
                        case '{':
                            yield return state.TokenWithPos(Tokens.OpenBrace);
                            state.Advance();
                            break;
                        case '}':
                            yield return state.TokenWithPos(Tokens.CloseBrace);
                            state.Advance();
                            break;
                        case ',':
                            yield return state.TokenWithPos(Tokens.Comma);
                            state.Advance();
                            break;
                        case ';':
                            yield return state.TokenWithPos(Tokens.Semicolon);
                            state.Advance();
                            break;
                        case ':':
                            yield return state.TokenWithPos(Tokens.Colon);
                            state.Advance();
                            break;
                        case '+':
                            yield return state.TokenWithPos(Tokens.Plus);
                            state.Advance();
                            break;
                        case '-':
                            yield return state.TokenWithPos(Tokens.Minus);
                            state.Advance();
                            break;
                        case '*':
                            yield return state.TokenWithPos(Tokens.Times);
                            state.Advance();
                            break;
                        case '/':
                            yield return state.TokenWithPos(Tokens.Divide);
                            state.Advance();
                            break;
                        case '<':
                            yield return state.TokenWithPos(Tokens.LessThan);
                            state.Advance();
                            break;
                        case '>':
                            yield return state.TokenWithPos(Tokens.GreaterThan);
                            state.Advance();
                            break;
                        case '?':
                            yield return state.TokenWithPos(Tokens.Question);
                            state.Advance();
                            break;
                        case '=':
                            yield return state.TokenWithPos(Tokens.Equals);
                            state.Advance();
                            break;
                        case '%':
                            yield return state.TokenWithPos(Tokens.Modulo);
                            state.Advance();
                            break;
                        default:
                            yield return ComplexToken(state);
                            break;
                    }
                }
                state.SkipWhiteSpaceAndComments();
            }
        }

        private TokenWithPos ComplexToken(State state)
        {
            if (Char.IsDigit(state.Current))
            {
                return Number(state);
            }
            else if (Char.IsLetter(state.Current) || state.Current == '_' || state.Current == '$')
            {
                return Identifier(state);
            }
            else if (state.Current == '\"')
            {
                return String(state);
            }
            else
            {
                throw state.Error(Messages.Lexer.UnexpectedCharacter);
            }
        }

        private TokenWithPos Number(State state)
        {
            state.SetMark();
            bool seenDot = false;
            bool inNumber() => (!seenDot && state.Current == '.') || Char.IsDigit(state.Current);
            while (!state.AtEnd && inNumber())
            {
                if (state.Current == '.')
                {
                    seenDot = true;
                }
                state.Advance();
            }

            string tokenString = state.GetFromMark();
            return state.TokenWithPos(
                Tokens.Number(double.Parse(tokenString)),
                tokenString.Length,
                state.Mark);
        }

        private TokenWithPos Identifier(State state)
        {
            state.SetMark();
            state.Advance();
            while (!state.AtEnd && (state.Current == '_' || Char.IsLetterOrDigit(state.Current)))
            {
                state.Advance();
            }
            var id = state.GetFromMark();
            var token = id switch {
                "def" => Tokens.Keywords.Def,
                "as" => Tokens.Keywords.As,
                "and" => Tokens.Keywords.And,
                "or" => Tokens.Keywords.Or,
                "if" => Tokens.Keywords.If,
                "then" => Tokens.Keywords.Then,
                "else" => Tokens.Keywords.Else,
                "end" => Tokens.Keywords.End,
                "elif" => Tokens.Keywords.Elif,
                "try" => Tokens.Keywords.Try,
                "catch" => Tokens.Keywords.Catch,
                "reduce" => Tokens.Keywords.Reduce,
                _ => MakeIdentifier(id)
            };
            return state.TokenWithPos(token, id.Length, state.Mark);
        }

        private Token MakeIdentifier(string id)
            => id.StartsWith('$') 
                ? Tokens.VariableIdentifier(id.Substring(1))
                : Tokens.Identifier(id);

        private TokenWithPos String(State state)
        {
            state.Advance();
            state.SetMark();
            while(true) {
                if (state.AtEnd) 
                {
                    throw state.Error(Messages.Lexer.EoiInsideString);
                }
                else if (state.Current == '\"')
                {
                    var value = state.GetFromMark();
                    state.Advance();
                    try 
                    {
                        var unescaped = Regex.Unescape(value);
                        return state.TokenWithPos(
                            new String(unescaped), value.Length + 2, state.Mark - 1);
                    }
                    catch (RegexParseException ex)
                    {
                        throw state.Error(
                            Messages.Lexer.InvalidEscapeSequence, state.Mark - 1, value.Length + 2, ex);
                    }
                }
                else if (state.Current == '\\' && state.Next != null)
                {
                    state.Advance(2);
                }
                else
                {
                    state.Advance();
                }
            }
        }
    }
}
