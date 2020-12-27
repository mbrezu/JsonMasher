using System;
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
            var result = lexer.Tokenize(program);

            // Assert
            AreTokenListsTheSame(result, expectedTokens).Should().BeTrue();
        }

        private static IEnumerable<TestItem> GetTestData()
            => Enumerable.Empty<TestItem>()
                .Concat(SimpleTokensTests())
                .Concat(NumberTests())
                .Concat(IdentfierTests());

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
        }

        private static List<Token> TokensParams(params Token[] tokens) => tokens.ToList();

        private static bool AreTokenListsTheSame(
            IEnumerable<Token> actual, IEnumerable<Token> expected)
        {
            var actualList = actual.ToList();
            var expectedList = expected.ToList();
            if (actualList.Count != expectedList.Count)
            {
                return false;
            }
            for (int i = 0; i < actualList.Count; i++)
            {
                var actualItem = actualList[i];
                var expectedItem = expectedList[i];
                if (!actualItem.SameAs(expectedItem))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
