namespace JsonMasher.Compiler
{
    public interface Token {}

    public record Identifier(string Id): Token;
    public record String(string Value): Token;
    public record Number(double Value): Token;

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
            DotDot
        }

        private enum KeywordType
        {
            Def,
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

        private static Token _dotDot = new SimpleToken(SimpleTokenType.DotDot);
        public static Token DotDot => _dotDot;

        public static Token Number(double v) => new Number(v);
        public static Token Identifier(string id) => new Identifier(id);

        public static bool SameAs(this Token t1, Token t2)
            => (t1, t2) switch
            {
                (SimpleToken st1, SimpleToken st2) => st1.Type == st2.Type,
                (Keyword kw1, Keyword kw2) => kw1.Type == kw2.Type,
                (Number n1, Number n2) => n1.Value == n2.Value,
                (Identifier id1, Identifier id2) => id1.Id == id2.Id,
                _ => false
            };
    }
}
