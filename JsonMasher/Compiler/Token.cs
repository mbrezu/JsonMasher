namespace JsonMasher.Compiler
{
    public interface Token {}

    record Identifier(string Id): Token;
    record VariableIdentifier(string Id): Token;
    record String(string Value): Token;
    record Number(double Value): Token;

    public static class Tokens
    {
        private enum SimpleTokenType
        {
            Dot,
            Pipe,
            OpenParen,
            CloseParen,
            OpenSquareParen,
            CloseSquareParen,
            OpenBrace,
            CloseBrace,
            Comma,
            Semicolon,
            Colon,
            DotDot,
            Plus,
            Minus,
            Times,
            EqualsEquals,
            PipeEquals,
            Divide
        }

        private enum KeywordType
        {
            Def,
            As
        }

        private record SimpleToken(SimpleTokenType Type): Token;
        private record Keyword(KeywordType Type): Token;

        private static Token _dot = new SimpleToken(SimpleTokenType.Dot);
        public static Token Dot => _dot;

        private static Token _pipe = new SimpleToken(SimpleTokenType.Pipe);
        public static Token Pipe => _pipe;

        private static Token _openParen = new SimpleToken(SimpleTokenType.OpenParen);
        public static Token OpenParen => _openParen;

        private static Token _closeParen = new SimpleToken(SimpleTokenType.CloseParen);
        public static Token CloseParen => _closeParen;

        private static Token _openSquareParen = new SimpleToken(SimpleTokenType.OpenSquareParen);
        public static Token OpenSquareParen => _openSquareParen;

        private static Token _closeSquareParen = new SimpleToken(SimpleTokenType.CloseSquareParen);
        public static Token CloseSquareParen => _closeSquareParen;

        private static Token _openBrace = new SimpleToken(SimpleTokenType.OpenBrace);
        public static Token OpenBrace => _openBrace;

        private static Token _closeBrace = new SimpleToken(SimpleTokenType.CloseBrace);
        public static Token CloseBrace => _closeBrace;

        private static Token _comma = new SimpleToken(SimpleTokenType.Comma);
        public static Token Comma => _comma;

        private static Token _semicolon = new SimpleToken(SimpleTokenType.Semicolon);
        public static Token Semicolon => _semicolon;

        private static Token _colon = new SimpleToken(SimpleTokenType.Colon);
        public static Token Colon => _colon;

        private static Token _plus = new SimpleToken(SimpleTokenType.Plus);
        public static Token Plus => _plus;

        private static Token _minus = new SimpleToken(SimpleTokenType.Minus);
        public static Token Minus => _minus;

        private static Token _times = new SimpleToken(SimpleTokenType.Times);
        public static Token Times => _times;

        private static Token _dotDot = new SimpleToken(SimpleTokenType.DotDot);
        public static Token DotDot => _dotDot;

        private static Token _equalsEquals = new SimpleToken(SimpleTokenType.EqualsEquals);
        public static Token EqualsEquals => _equalsEquals;

        private static Token _pipeEquals = new SimpleToken(SimpleTokenType.PipeEquals);
        public static Token PipeEquals => _pipeEquals;

        private static Token _divide = new SimpleToken(SimpleTokenType.Divide);
        public static Token Divide => _divide;

        public static Token Number(double value) => new Number(value);
        public static Token Identifier(string id) => new Identifier(id);
        public static Token VariableIdentifier(string id) => new VariableIdentifier(id);
        public static Token String(string value) => new String(value);
        
        public static class Keywords {
            private static Token _def = new Keyword(KeywordType.Def);
            public static Token Def => _def;
            private static Token _as = new Keyword(KeywordType.As);
            public static Token As => _as;
        }
    }
}
