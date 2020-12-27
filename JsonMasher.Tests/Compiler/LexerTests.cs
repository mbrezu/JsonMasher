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
        public void TestDeepEqual(string program, List<Token> expectedTokens)
        {
            // Arrange
            var lexer = new Lexer();

            // Act
            var result = lexer.Tokenize(program);

            // Assert
            result.Should().BeEquivalentTo(
                expectedTokens, options => options.AllowingInfiniteRecursion());
        }

        private static IEnumerable<TestItem> GetTestData()
            => SimpleTokensTests();

        private static IEnumerable<TestItem> SimpleTokensTests()
        {
            yield return new TestItem("", Enumerable.Empty<Token>().ToList());
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
        }

        private static List<Token> TokensParams(params Token[] tokens) => tokens.ToList();
    }
}
