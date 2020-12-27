using System;
using System.Collections.Generic;

namespace JsonMasher.Compiler
{
    public class Lexer : ILexer
    {
        class State
        {
            private string _program;
            private int _index;

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
                else 
                {
                    yield return state.Char switch {
                        '.' => Tokens.Dot,
                        '|' => Tokens.Pipe,
                        '(' => Tokens.OpenParen,
                        ')' => Tokens.CloseParen,
                        '[' => Tokens.OpenSquareParen,
                        ']' => Tokens.CloseSquareParen,
                        '{' => Tokens.OpenBrace,
                        '}' => Tokens.CloseBrace,
                        ',' => Tokens.Comma,
                        ';' => Tokens.Semicolon,
                        ':' => Tokens.Colon,
                        _ => throw new InvalidOperationException()
                    };
                    state.Advance();
                }
                state.SkipSpaces();
            }
        }
    }
}
