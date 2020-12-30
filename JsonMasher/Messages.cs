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
        public static class Parser
        {
            public const string ExtraInput = "Extra tokens in input.";
            public const string EmptyParameterList = "Remove the '()'.";
            public const string UnknownConstruct = "Unknown construct.";
            public const string FilterExpected = "Filter expected.";
        }
    }
}