using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Compiler;
using Xunit;

namespace JsonMasher.Tests.Compiler
{
    public class LexerTests
    {
        private record TestItem(string program, List<Token> expectedTokens);

        public static IEnumerable<object[]> TestData
            => GetTestData().Select(item => new object[] { item.program, item.expectedTokens });

        [Theory]
        [MemberData(nameof(TestData))]
        public void TokenizeTest(string program, List<Token> expectedTokens)
        {
            // Arrange
            var lexer = new Lexer();

            // Act
            var result = lexer.Tokenize(program).Select(t => t.Token);

            // Assert
            result.Should().BeEquivalentTo(
                expectedTokens, 
                options => options.RespectingRuntimeTypes().WithStrictOrdering());
        }

        private static IEnumerable<TestItem> GetTestData()
            => Enumerable.Empty<TestItem>()
                .Concat(SimpleTokensTests())
                .Concat(CommentsTests())
                .Concat(NumberTests())
                .Concat(IdentfierTests())
                .Concat(KeywordTests())
                .Concat(StringTests())
                .Concat(Tests());

        private static IEnumerable<TestItem> SimpleTokensTests()
        {
            yield return new TestItem("", TokensParams());
            yield return new TestItem(".", TokensParams(Tokens.Dot));
            yield return new TestItem("  . ", TokensParams(Tokens.Dot));
            yield return new TestItem("  . . ", TokensParams(Tokens.Dot, Tokens.Dot));
            yield return new TestItem("  .. ", TokensParams(Tokens.DotDot));
            yield return new TestItem("  .. ..", TokensParams(Tokens.DotDot, Tokens.DotDot));
            yield return new TestItem("  ( )", TokensParams(Tokens.OpenParen, Tokens.CloseParen));
            yield return new TestItem(
                "  ] [", TokensParams(Tokens.CloseSquareParen, Tokens.OpenSquareParen));
            yield return new TestItem(
                "  { }", TokensParams(Tokens.OpenBrace, Tokens.CloseBrace));
            yield return new TestItem("|", TokensParams(Tokens.Pipe));
            yield return new TestItem(
                ", : ;", TokensParams(Tokens.Comma, Tokens.Colon, Tokens.Semicolon));
            yield return new TestItem(
                "+ - *", TokensParams(Tokens.Plus, Tokens.Minus, Tokens.Times));
            yield return new TestItem(
                "== +", TokensParams(Tokens.EqualsEquals, Tokens.Plus));
            yield return new TestItem(
                "!= +", TokensParams(Tokens.NotEquals, Tokens.Plus));
            yield return new TestItem(
                "|= +", TokensParams(Tokens.PipeEquals, Tokens.Plus));
            yield return new TestItem(
                "/ +", TokensParams(Tokens.Divide, Tokens.Plus));
            yield return new TestItem(
                "> <", TokensParams(Tokens.GreaterThan, Tokens.LessThan));
            yield return new TestItem(
                "<= >=", TokensParams(Tokens.LessThanOrEqual, Tokens.GreaterThanOrEqual));
            yield return new TestItem(
                "? >=", TokensParams(Tokens.Question, Tokens.GreaterThanOrEqual));
            yield return new TestItem(
                "? //", TokensParams(Tokens.Question, Tokens.SlashSlash));
            yield return new TestItem("=", TokensParams(Tokens.Equals));
            yield return new TestItem("%", TokensParams(Tokens.Modulo));
        }

        private static IEnumerable<TestItem> CommentsTests()
        {
            yield return new TestItem(
                "<= # >=", TokensParams(Tokens.LessThanOrEqual));
            yield return new TestItem(
                @"<= # comment 
>=", TokensParams(Tokens.LessThanOrEqual, Tokens.GreaterThanOrEqual));
        }

        private static IEnumerable<TestItem> NumberTests()
        {
            yield return new TestItem(" 0 ", TokensParams(Tokens.Number(0)));
            yield return new TestItem(" 0", TokensParams(Tokens.Number(0)));
            yield return new TestItem(" 0. ", TokensParams(Tokens.Number(0)));
            yield return new TestItem(" 0.10 ", TokensParams(Tokens.Number(0.1)));
            yield return new TestItem(" 102 ", TokensParams(Tokens.Number(102)));
            yield return new TestItem(" 102.3 ", TokensParams(Tokens.Number(102.3)));
            yield return new TestItem(
                " 102.3.. ", TokensParams(Tokens.Number(102.3), Tokens.DotDot));
            yield return new TestItem(
                " .5 .2 ", TokensParams(Tokens.Number(.5), Tokens.Number(.2)));
        }

        private static IEnumerable<TestItem> IdentfierTests()
        {
            yield return new TestItem(" a ", TokensParams(Tokens.Identifier("a")));
            yield return new TestItem(" a", TokensParams(Tokens.Identifier("a")));
            yield return new TestItem(" _a", TokensParams(Tokens.Identifier("_a")));
            yield return new TestItem(" _a1", TokensParams(Tokens.Identifier("_a1")));
            yield return new TestItem(" snake_case", TokensParams(Tokens.Identifier("snake_case")));
            yield return new TestItem(
                " snake_case, PascalCase   \t", 
                TokensParams(
                    Tokens.Identifier("snake_case"),
                    Tokens.Comma,
                    Tokens.Identifier("PascalCase")));
            yield return new TestItem(" $test", TokensParams(Tokens.VariableIdentifier("test")));
        }

        private static IEnumerable<TestItem> KeywordTests()
        {
            yield return new TestItem(" def ", TokensParams(Tokens.Keywords.Def));
            yield return new TestItem(" as ", TokensParams(Tokens.Keywords.As));
            yield return new TestItem(" and ", TokensParams(Tokens.Keywords.And));
            yield return new TestItem(" or ", TokensParams(Tokens.Keywords.Or));
            yield return new TestItem(" if ", TokensParams(Tokens.Keywords.If));
            yield return new TestItem(" then ", TokensParams(Tokens.Keywords.Then));
            yield return new TestItem(" else ", TokensParams(Tokens.Keywords.Else));
            yield return new TestItem(" end ", TokensParams(Tokens.Keywords.End));
            yield return new TestItem(" elif ", TokensParams(Tokens.Keywords.Elif));
            yield return new TestItem(" try ", TokensParams(Tokens.Keywords.Try));
            yield return new TestItem(" catch ", TokensParams(Tokens.Keywords.Catch));
            yield return new TestItem(" reduce ", TokensParams(Tokens.Keywords.Reduce));
            yield return new TestItem(" foreach ", TokensParams(Tokens.Keywords.Foreach));
        }

        private static IEnumerable<TestItem> StringTests()
        {
            yield return new TestItem(" \"a\" ", TokensParams(Tokens.String("a")));
            yield return new TestItem(
                " \"some string {}{-\" ", TokensParams(Tokens.String("some string {}{-")));
            yield return new TestItem(
                " \"some string {}{-\".. ", 
                TokensParams(
                    Tokens.String("some string {}{-"),
                    Tokens.DotDot));
            yield return new TestItem(
                " 23 def  \t\n\"some string {}{-\". ", 
                TokensParams(
                    Tokens.Number(23),
                    Tokens.Keywords.Def,
                    Tokens.String("some string {}{-"),
                    Tokens.Dot));
            yield return new TestItem(
                @" ""escaped \"" double quotes""", 
                TokensParams(Tokens.String("escaped \" double quotes")));
            yield return new TestItem(
                @" ""escapes 1 \n""", 
                TokensParams(Tokens.String("escapes 1 \n")));
        }

        private static IEnumerable<TestItem> Tests()
        {
            yield return new TestItem(
                ".[] | ..", 
                TokensParams(
                    Tokens.Dot,
                    Tokens.OpenSquareParen,
                    Tokens.CloseSquareParen,
                    Tokens.Pipe,
                    Tokens.DotDot));
            yield return new TestItem(
                "[ range(3) ]", 
                TokensParams(
                    Tokens.OpenSquareParen,
                    Tokens.Identifier("range"),
                    Tokens.OpenParen,
                    Tokens.Number(3),
                    Tokens.CloseParen,
                    Tokens.CloseSquareParen));
        }

        private static List<Token> TokensParams(params Token[] tokens) => tokens.ToList();
    }
}
