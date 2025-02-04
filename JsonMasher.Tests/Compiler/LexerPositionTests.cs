using System.Collections.Generic;
using System.Linq;
using JsonMasher.Compiler;
using Shouldly;
using Xunit;

namespace JsonMasher.Tests.Compiler
{
    public class LexerPositionTests
    {
        private record TestItem(string program, List<TokenWithPos> expectedTokens);

        public static IEnumerable<object[]> TestData =>
            GetTestData().Select(item => new object[] { item.program, item.expectedTokens });

        [Theory]
        [MemberData(nameof(TestData))]
        public void TokenizeTest(string program, List<TokenWithPos> expectedTokens)
        {
            // Arrange
            var lexer = new Lexer();

            // Act
            var result = lexer.Tokenize(program);

            // Assert
            result.ShouldBe(expectedTokens);
        }

        private static IEnumerable<TestItem> GetTestData() =>
            Enumerable.Empty<TestItem>().Concat(SimplePosTests());

        private static IEnumerable<TestItem> SimplePosTests()
        {
            yield return new TestItem("", TokensParams());
            yield return new TestItem(".", TokensParams(new TokenWithPos(Tokens.Dot, 0, 1)));
            yield return new TestItem("..", TokensParams(new TokenWithPos(Tokens.DotDot, 0, 2)));
            yield return new TestItem(
                "..  ..",
                TokensParams(
                    new TokenWithPos(Tokens.DotDot, 0, 2),
                    new TokenWithPos(Tokens.DotDot, 4, 6)
                )
            );
            yield return new TestItem(
                "123",
                TokensParams(new TokenWithPos(Tokens.Number(123), 0, 3))
            );
            yield return new TestItem(
                "abc",
                TokensParams(new TokenWithPos(Tokens.Identifier("abc"), 0, 3))
            );
            yield return new TestItem(
                "$abc",
                TokensParams(new TokenWithPos(Tokens.VariableIdentifier("abc"), 0, 4))
            );
            yield return new TestItem(
                "\"abc\"",
                TokensParams(new TokenWithPos(Tokens.String("abc"), 0, 5))
            );
            yield return new TestItem(
                "\"a\\tbc\"",
                TokensParams(new TokenWithPos(Tokens.String("a\tbc"), 0, 7))
            );
            yield return new TestItem(
                "def map(x): .[] x;",
                TokensParams(
                    new TokenWithPos(Tokens.Keywords.Def, 0, 3),
                    new TokenWithPos(Tokens.Identifier("map"), 4, 7),
                    new TokenWithPos(Tokens.OpenParen, 7, 8),
                    new TokenWithPos(Tokens.Identifier("x"), 8, 9),
                    new TokenWithPos(Tokens.CloseParen, 9, 10),
                    new TokenWithPos(Tokens.Colon, 10, 11),
                    new TokenWithPos(Tokens.Dot, 12, 13),
                    new TokenWithPos(Tokens.OpenSquareParen, 13, 14),
                    new TokenWithPos(Tokens.CloseSquareParen, 14, 15),
                    new TokenWithPos(Tokens.Identifier("x"), 16, 17),
                    new TokenWithPos(Tokens.Semicolon, 17, 18)
                )
            );
        }

        private static List<TokenWithPos> TokensParams(params TokenWithPos[] tokens) =>
            tokens.ToList();
    }
}
