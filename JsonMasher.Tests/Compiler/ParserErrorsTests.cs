using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Compiler;
using Xunit;
using System;

namespace JsonMasher.Tests.Compiler
{
    public class ParserErrorsTests
    {
        public record PositionInformation(int Line, int Column, string Highlights);
        public record TestItem(
            string Program,
            string MessageExpectation,
            PositionInformation PositionInformationExpectation);

        public static IEnumerable<object[]> TestData
            => GetTestData().Select(item => new object[] {
                item.Program, 
                item.MessageExpectation,
                item.PositionInformationExpectation
            });

        [Theory]
        [MemberData(nameof(TestData))]
        public void ParseTest(
            string program,
            string messageExpectation,
            PositionInformation positionInformationExpectation)
        {
            // Arrange
            var parser = new Parser();

            // Act
            Action action = () => parser.Parse(program);

            // Assert
            action
                .Should().Throw<JsonMasherException>()
                .Where(e => VerifyException(e, messageExpectation, positionInformationExpectation));
        }

        private static IEnumerable<TestItem> GetTestData()
            => Enumerable.Empty<TestItem>()
                .Concat(ErrorTests());

        private static IEnumerable<TestItem> ErrorTests()
        {
            yield return new TestItem(
                "def test|",
                "Expected ':', but got '|'.",
                new PositionInformation(
                    1, 9,
                    @"Line 1: def test|
Line 1:         ^
"
                ));
            yield return new TestItem(
                "def test",
                "Expected ':', but reached end of input.",
                new PositionInformation(
                    1, 9,
                    @""
                ));
            yield return new TestItem(
                "def test as",
                "Expected ':', but got 'as'.",
                new PositionInformation(
                    1, 10,
                    @"Line 1: def test as
Line 1:          ^^
"
                ));
            yield return new TestItem(
                "1 1",
                Messages.Parser.ExtraInput,
                new PositionInformation(
                    1, 3,
                    @"Line 1: 1 1
Line 1:   ^
"
                ));
            yield return new TestItem(
                "def 1",
                "Expected an identifier, but got '1'.",
                new PositionInformation(
                    1, 5,
                    @"Line 1: def 1
Line 1:     ^
"
                ));
            yield return new TestItem(
                "def test()",
                Messages.Parser.EmptyParameterList,
                new PositionInformation(
                    1, 10,
                    @"Line 1: def test()
Line 1:          ^
"
                ));
            yield return new TestItem(
                "def test(1)",
                "Expected an identifier or a variable identifier (e.g. '$a'), but got '1'.",
                new PositionInformation(
                    1, 10,
                    @"Line 1: def test(1)
Line 1:          ^
"
                ));
            yield return new TestItem(
                "def test(a;)",
                "Expected an identifier or a variable identifier (e.g. '$a'), but got ')'.",
                new PositionInformation(
                    1, 12,
                    @"Line 1: def test(a;)
Line 1:            ^
"
                ));
            yield return new TestItem(
                "def test(a;",
                "Expected an identifier or a variable identifier (e.g. '$a'), but reached end of input.",
                new PositionInformation(
                    1, 12,
                    @""
                ));
            yield return new TestItem(
                "1 as",
                "Expected a variable identifier (e.g. '$a'), but reached end of input.",
                new PositionInformation(
                    1, 5,
                    @""
                ));
            yield return new TestItem(
                "1 as a",
                "Expected a variable identifier (e.g. '$a'), but got 'a'.",
                new PositionInformation(
                    1, 6,
                    @"Line 1: 1 as a
Line 1:      ^
"
                ));
            yield return new TestItem(
                "+",
                Messages.Parser.UnknownConstruct,
                new PositionInformation(
                    1, 1,
                    @"Line 1: +
Line 1: ^
"
                ));
            yield return new TestItem(
                "a()",
                Messages.Parser.EmptyParameterList,
                new PositionInformation(
                    1, 3,
                    @"Line 1: a()
Line 1:   ^
"
                ));
            yield return new TestItem(
                "a(1;)",
                Messages.Parser.FilterExpected,
                new PositionInformation(
                    1, 5,
                    @"Line 1: a(1;)
Line 1:     ^
"
                ));
            yield return new TestItem(
                "if a then b stuff",
                "Expected 'else' or 'elif', but got 'stuff'.",
                new PositionInformation(
                    1, 13,
                    @"Line 1: if a then b stuff
Line 1:             ^^^^^
"
                ));
            yield return new TestItem(
                "if a then b",
                "Expected 'else' or 'elif', but reached end of input.",
                new PositionInformation(
                    1, 12,
                    @""
                ));
            yield return new TestItem(
                "{ 2",
                "Expected ':', but reached end of input.",
                new PositionInformation(
                    1, 4,
                    @""
                ));
            yield return new TestItem(
                "{",
                "Unknown construct.",
                new PositionInformation(
                    1, 2,
                    @""
                ));
            yield return new TestItem(
                "{ a:2, }",
                "Expected a key-value pair (e.g. 'a:1'), but got '}'.",
                new PositionInformation(
                    1, 8,
                    @"Line 1: { a:2, }
Line 1:        ^
"
                ));
        }

        private bool VerifyException(
            JsonMasherException ex,
            string messageExpectation,
            PositionInformation positionInformationExpectation)
        {
            ex.Message.Should().Be(messageExpectation);
            if (positionInformationExpectation != null)
            {
                ex.Line.Should().Be(positionInformationExpectation.Line);
                ex.Column.Should().Be(positionInformationExpectation.Column);
                ex.Highlights.CleanCR().Should().Be(positionInformationExpectation.Highlights.CleanCR());
            }
            return true;
        }
    }
}
