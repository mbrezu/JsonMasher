using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Compiler;
using JsonMasher.Mashers;
using Xunit;

namespace JsonMasher.Tests.EndToEnd
{
    public class MashtimeErrors
    {
        public record PositionInformation(int Line, int Column, string Highlights);
        private record TestItem(
            string Program, 
            string InputJson, 
            string ExpectedMessage,
            PositionInformation ExpectedPositionInformation);

        public static IEnumerable<object[]> TestData
            => GetTestData().Select((System.Func<TestItem, object[]>)(item => (new object[] {
                item.Program, 
                item.InputJson,
                item.ExpectedMessage,
                item.ExpectedPositionInformation
            })));

        [Theory]
        [MemberData(nameof(TestData))]
        public void ProgramTest(
            string program,
            string inputJson,
            string expectedMessage,
            PositionInformation expectedPositionInformation)
        {
            // Arrange
            var parser = new Parser();
            var (filter, sourceInformation) = parser.Parse(program, new SequenceGenerator());
            var input = inputJson.AsJson().AsEnumerable();

            // Act
            Action action = () => {
                var (result, context) = new Mashers.JsonMasher().Mash(
                    input, filter, new DebugMashStack(), sourceInformation);
                result.ToList();
            };

            // Assert
            action
                .Should().Throw<JsonMasherException>()
                .Where(ex => ValidateException(ex, expectedMessage, expectedPositionInformation));
        }

        private bool ValidateException(
            JsonMasherException ex,
            string expectedMessage,
            PositionInformation expectedPositionInformation)
        {
            ex.Message.Should().Be(expectedMessage);
            if (expectedPositionInformation != null)
            {
                ex.Line.Should().Be(expectedPositionInformation.Line);
                ex.Column.Should().Be(expectedPositionInformation.Column);
                ex.Highlights.Should().Be(expectedPositionInformation.Highlights);
            }
            return true;
        }

        private static IEnumerable<TestItem> GetTestData()
            => Enumerable.Empty<TestItem>()
                .Concat(SimplePrograms());

        private static IEnumerable<TestItem> SimplePrograms()
        {
            yield return new TestItem(
                "1 + [1, 2]",
                "null",
                "Can't add Number and Array.",
                new PositionInformation(1, 1, @"Line 1: 1 + [1, 2]
Line 1: ^^^^^^^^^^
"));
            yield return new TestItem(
                "1, 1 + [1, 2]",
                "null",
                "Can't add Number and Array.",
                new PositionInformation(1, 4, @"Line 1: 1, 1 + [1, 2]
Line 1:    ^^^^^^^^^^
Line 1: 1, 1 + [1, 2]
Line 1: ^^^^^^^^^^^^^
"));
            yield return new TestItem(
                "1 | 1 + [1, 2]",
                "null",
                "Can't add Number and Array.",
                new PositionInformation(1, 5, @"Line 1: 1 | 1 + [1, 2]
Line 1:     ^^^^^^^^^^
Line 1: 1 | 1 + [1, 2]
Line 1: ^^^^^^^^^^^^^^
"));
            yield return new TestItem(
                ".[1 + [1, 2]]",
                "null",
                "Can't add Number and Array.",
                new PositionInformation(1, 3, @"Line 1: .[1 + [1, 2]]
Line 1:   ^^^^^^^^^^
Line 1: .[1 + [1, 2]]
Line 1: ^^^^^^^^^^^^^
"));
            yield return new TestItem(
                "[1 + [1, 2]]",
                "null",
                "Can't add Number and Array.",
                new PositionInformation(1, 2, @"Line 1: [1 + [1, 2]]
Line 1:  ^^^^^^^^^^
Line 1: [1 + [1, 2]]
Line 1: ^^^^^^^^^^^^
"));
            yield return new TestItem(
                "{a: 1 + [1, 2]}",
                "null",
                "Can't add Number and Array.",
                new PositionInformation(1, 5, @"Line 1: {a: 1 + [1, 2]}
Line 1:     ^^^^^^^^^^
Line 1: {a: 1 + [1, 2]}
Line 1: ^^^^^^^^^^^^^^^
"));
            yield return new TestItem(
                "if true then 1 + [1, 2] else 2 end",
                "null",
                "Can't add Number and Array.",
                new PositionInformation(1, 14, @"Line 1: if true then 1 + [1, 2] else 2 end
Line 1:              ^^^^^^^^^^
Line 1: if true then 1 + [1, 2] else 2 end
Line 1: ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
"));
            yield return new TestItem(
                "1 + [1, 2] as $test | $test",
                "null",
                "Can't add Number and Array.",
                new PositionInformation(1, 1, @"Line 1: 1 + [1, 2] as $test | $test
Line 1: ^^^^^^^^^^
Line 1: 1 + [1, 2] as $test | $test
Line 1: ^^^^^^^^^^^^^^^^^^^^^^^^^^^
"));
            yield return new TestItem(
                ". |= 1 + [1, 2]",
                "null",
                "Can't add Number and Array.",
                new PositionInformation(1, 6, @"Line 1: . |= 1 + [1, 2]
Line 1:      ^^^^^^^^^^
Line 1: . |= 1 + [1, 2]
Line 1: ^^^^^^^^^^^^^^^
"));
            yield return new TestItem(
                "200 * [1, 2]",
                "null",
                "Can't multiply Number and Array.",
                new PositionInformation(1, 1, @"Line 1: 200 * [1, 2]
Line 1: ^^^^^^^^^^^^
"));
            yield return new TestItem(
                "200 / [1, 2]",
                "null",
                "Can't divide Number and Array.",
                new PositionInformation(1, 1, @"Line 1: 200 / [1, 2]
Line 1: ^^^^^^^^^^^^
"));
            yield return new TestItem(
                "200 > [1, 2]",
                "null",
                "Can't compare Number and Array.",
                new PositionInformation(1, 1, @"Line 1: 200 > [1, 2]
Line 1: ^^^^^^^^^^^^
"));
            yield return new TestItem(
                "200 >= [1, 2]",
                "null",
                "Can't compare Number and Array.",
                new PositionInformation(1, 1, @"Line 1: 200 >= [1, 2]
Line 1: ^^^^^^^^^^^^^
"));
            yield return new TestItem(
                "200 < [1, 2]",
                "null",
                "Can't compare Number and Array.",
                new PositionInformation(1, 1, @"Line 1: 200 < [1, 2]
Line 1: ^^^^^^^^^^^^
"));
            yield return new TestItem(
                "200 <= [1, 2]",
                "null",
                "Can't compare Number and Array.",
                new PositionInformation(1, 1, @"Line 1: 200 <= [1, 2]
Line 1: ^^^^^^^^^^^^^
"));
            yield return new TestItem(
                "limit([1]; 1, 2, 3)",
                "null",
                "Can't use Array as limit.",
                new PositionInformation(1, 1, @"Line 1: limit([1]; 1, 2, 3)
Line 1: ^^^^^^^^^^^^^^^^^^^
"));
            yield return new TestItem(
                "- [1, 2]",
                "null",
                "Can't make negative an Array.",
                new PositionInformation(1, 1, @"Line 1: - [1, 2]
Line 1: ^^^^^^^^
"));
            yield return new TestItem(
                "100 - [1, 2]",
                "null",
                "Can't subtract Array from Number.",
                new PositionInformation(1, 1, @"Line 1: 100 - [1, 2]
Line 1: ^^^^^^^^^^^^
"));
            yield return new TestItem(
                "(1 | 2) |= 1",
                "null",
                "Not a path expression.",
                new PositionInformation(1, 2, @"Line 1: (1 | 2) |= 1
Line 1:  ^
Line 1: (1 | 2) |= 1
Line 1:  ^^^^^
Line 1: (1 | 2) |= 1
Line 1: ^^^^^^^^^^^^
"));
            yield return new TestItem(
                "range",
                "null",
                "Function range/0 is not known.",
                new PositionInformation(1, 1, @"Line 1: range
Line 1: ^^^^^
"));
            yield return new TestItem(
                "def a(x; y): .; a(2)",
                "null",
                "Function a/1 is not known.",
                new PositionInformation(1, 17, @"Line 1: def a(x; y): .; a(2)
Line 1:                 ^^^^
Line 1: def a(x; y): .; a(2)
Line 1: ^^^^^^^^^^^^^^^^^^^^
"));
            yield return new TestItem(
                "10 |= 2",
                "null",
                "Not a path expression.",
                new PositionInformation(1, 1, @"Line 1: 10 |= 2
Line 1: ^^
Line 1: 10 |= 2
Line 1: ^^^^^^^
"));
            yield return new TestItem(
                "$test",
                "null",
                "Cannot find variable $test.",
                new PositionInformation(1, 1, @"Line 1: $test
Line 1: ^^^^^
"));
            yield return new TestItem(
                "0 | .[]",
                "null",
                "Can't enumerate Number.",
                new PositionInformation(1, 5, @"Line 1: 0 | .[]
Line 1:     ^^^
Line 1: 0 | .[]
Line 1: ^^^^^^^
"));
            yield return new TestItem(
                "0 | .[] |= 2",
                "null",
                "Can't enumerate Number.",
                new PositionInformation(1, 5, @"Line 1: 0 | .[] |= 2
Line 1:     ^^^
Line 1: 0 | .[] |= 2
Line 1:     ^^^^^^^^
Line 1: 0 | .[] |= 2
Line 1: ^^^^^^^^^^^^
"));
            yield return new TestItem(
                "0 | .a",
                "null",
                "Can't index Number with a string.",
                new PositionInformation(1, 5, @"Line 1: 0 | .a
Line 1:     ^^
Line 1: 0 | .a
Line 1: ^^^^^^
"));
            yield return new TestItem(
                "0 | .\"a\"",
                "null",
                "Can't index Number with a string.",
                new PositionInformation(1, 5, @"Line 1: 0 | .""a""
Line 1:     ^^^^
Line 1: 0 | .""a""
Line 1: ^^^^^^^^
"));
            yield return new TestItem(
                "{} | .[0]",
                "null",
                "Can't index Object with Number.",
                new PositionInformation(1, 6, @"Line 1: {} | .[0]
Line 1:      ^^^^
Line 1: {} | .[0]
Line 1: ^^^^^^^^^
"));
            yield return new TestItem(
                "{} | .[0] |= 1",
                "null",
                "Can't index a Object with a Number key.",
                new PositionInformation(1, 6, @"Line 1: {} | .[0] |= 1
Line 1:      ^^^^
Line 1: {} | .[0] |= 1
Line 1:      ^^^^^^^^^
Line 1: {} | .[0] |= 1
Line 1: ^^^^^^^^^^^^^^
"));
            yield return new TestItem(
                "[] | .[\"a\"] |= 1",
                "null",
                "Can't index a Array with a String key.",
                new PositionInformation(1, 6, @"Line 1: [] | .[""a""] |= 1
Line 1:      ^^^^^^
Line 1: [] | .[""a""] |= 1
Line 1:      ^^^^^^^^^^^
Line 1: [] | .[""a""] |= 1
Line 1: ^^^^^^^^^^^^^^^^
"));
            yield return new TestItem(
                "1 | .[\"a\"] |= 1",
                "null",
                "Can't index a Number with a String key.",
                new PositionInformation(1, 5, @"Line 1: 1 | .[""a""] |= 1
Line 1:     ^^^^^^
Line 1: 1 | .[""a""] |= 1
Line 1:     ^^^^^^^^^^^
Line 1: 1 | .[""a""] |= 1
Line 1: ^^^^^^^^^^^^^^^
"));
            yield return new TestItem(
                "keys",
                "100",
                "Number has no keys.",
                new PositionInformation(1, 1, @"Line 1: keys
Line 1: ^^^^
"));
        }
    }
}
