namespace JsonMasher
{
    public static class Messages
    {
        public static class Lexer
        {
            public const string InvalidEscapeSequence = "Invalid escape sequence in string.";
            public const string EoiInsideString = "Expected '\"', but reached the end of input.";
            public const string UnexpectedCharacter = "Unexpected character.";
        }
    }
}