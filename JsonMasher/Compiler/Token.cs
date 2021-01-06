using System.ComponentModel.DataAnnotations;

namespace JsonMasher.Compiler
{
    public interface Token 
    {
        public string GetDisplayNameForException();
    }

    record Identifier(string Id): Token
    {
        public string GetDisplayNameForException() => $"'{Id}'";
    }

    record VariableIdentifier(string Id): Token
    {
        public string GetDisplayNameForException() => $"'${Id}'";
    }

    record String(string Value): Token
    {
        public string GetDisplayNameForException() => $"\"{Value}\"";
    }

    record Number(double Value): Token
    {
        public string GetDisplayNameForException() => $"'{Value}'";
    }

    public record TokenWithPos(Token Token, int StartPos, int EndPos);

    public static class Tokens
    {
        private enum SimpleTokenType
        {
            [Display(Name=".")]
            Dot,
            [Display(Name="|")]
            Pipe,
            [Display(Name="(")]
            OpenParen,
            [Display(Name=")")]
            CloseParen,
            [Display(Name="[")]
            OpenSquareParen,
            [Display(Name="]")]
            CloseSquareParen,
            [Display(Name="{")]
            OpenBrace,
            [Display(Name="}")]
            CloseBrace,
            [Display(Name=",")]
            Comma,
            [Display(Name=";")]
            Semicolon,
            [Display(Name=":")]
            Colon,
            [Display(Name="..")]
            DotDot,
            [Display(Name="+")]
            Plus,
            [Display(Name="-")]
            Minus,
            [Display(Name="*")]
            Times,
            [Display(Name="==")]
            EqualsEquals,
            [Display(Name="|=")]
            PipeEquals,
            [Display(Name="/")]
            Divide,
            [Display(Name="<")]
            LessThan,
            [Display(Name="<=")]
            LessThanOrEqual,
            [Display(Name=">")]
            GreaterThan,
            [Display(Name=">=")]
            GreaterThanOrEqual,
            [Display(Name="?")]
            Question,
            [Display(Name="!=")]
            NotEquals,
            [Display(Name="//")]
            SlashSlash,
            [Display(Name="=")]
            Equals,
            [Display(Name="%")]
            Modulo
        }

        private enum KeywordType
        {
            Def,
            As,
            And,
            Or,
            If,
            Then,
            Else,
            End,
            Elif,
            Try,
            Catch,
            Reduce
        }

        private record SimpleToken(SimpleTokenType Type): Token
        {
            public string GetDisplayNameForException() => $"'{Type.GetEnumDisplayName()}'";
        }

        private record Keyword(KeywordType Type): Token
        {
            public string GetDisplayNameForException() => $"'{Type.ToString().ToLower()}'";
        }

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

        private static Token _lessThan = new SimpleToken(SimpleTokenType.LessThan);
        public static Token LessThan => _lessThan;

        private static Token _lessThanOrEqual = new SimpleToken(SimpleTokenType.LessThanOrEqual);
        public static Token LessThanOrEqual => _lessThanOrEqual;

        private static Token _greaterThan = new SimpleToken(SimpleTokenType.GreaterThan);
        public static Token GreaterThan => _greaterThan;

        private static Token _greaterThanOrEqual = new SimpleToken(SimpleTokenType.GreaterThanOrEqual);
        public static Token GreaterThanOrEqual => _greaterThanOrEqual;

        private static Token _question = new SimpleToken(SimpleTokenType.Question);
        public static Token Question => _question;

        private static Token _notEquals = new SimpleToken(SimpleTokenType.NotEquals);
        public static Token NotEquals => _notEquals;

        private static Token _slashSlash = new SimpleToken(SimpleTokenType.SlashSlash);
        public static Token SlashSlash => _slashSlash;

        private static Token _equals = new SimpleToken(SimpleTokenType.Equals);
        public static new Token Equals => _equals;

        private static Token _modulo = new SimpleToken(SimpleTokenType.Modulo);
        public static Token Modulo => _modulo;

        public static Token Number(double value) => new Number(value);
        public static Token Identifier(string id) => new Identifier(id);
        public static Token VariableIdentifier(string id) => new VariableIdentifier(id);
        public static Token String(string value) => new String(value);
        
        public static class Keywords {
            private static Token _def = new Keyword(KeywordType.Def);
            public static Token Def => _def;
            private static Token _as = new Keyword(KeywordType.As);
            public static Token As => _as;
            private static Token _and = new Keyword(KeywordType.And);
            public static Token And => _and;
            private static Token _or = new Keyword(KeywordType.Or);
            public static Token Or => _or;
            private static Token _if = new Keyword(KeywordType.If);
            public static Token If => _if;
            private static Token _then = new Keyword(KeywordType.Then);
            public static Token Then => _then;
            private static Token _else = new Keyword(KeywordType.Else);
            public static Token Else => _else;
            private static Token _end = new Keyword(KeywordType.End);
            public static Token End => _end;
            private static Token _elif = new Keyword(KeywordType.Elif);
            public static Token Elif => _elif;
            private static Token _try = new Keyword(KeywordType.Try);
            public static Token Try => _try;
            private static Token _catch = new Keyword(KeywordType.Catch);
            public static Token Catch => _catch;
            private static Token _reduce = new Keyword(KeywordType.Reduce);
            public static Token Reduce => _reduce;
        }
    }
}
