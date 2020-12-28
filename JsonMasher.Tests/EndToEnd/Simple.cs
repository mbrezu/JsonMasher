using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Compiler;
using Xunit;

namespace JsonMasher.Tests.EndToEnd
{
    public class Simple
    {
        private record TestItem(string Program, string InputJson, string ExpectedOutputJson);

        public static IEnumerable<object[]> TestData
            => GetTestData().Select((System.Func<TestItem, object[]>)(item => (new object[] {
                item.Program, item.InputJson, item.ExpectedOutputJson })));

        [Theory]
        [MemberData(nameof(TestData))]
        public void ProgramTest(
            string program, string inputJson, string expectedOutputJson)
        {
            // Arrange
            var parser = new Parser();
            var filter = parser.Parse(program);
            var input = inputJson.AsJson().AsEnumerable();

            // Act
            var result = new Mashers.JsonMasher().Mash(input, filter);

            // Assert
            Json.Array(result)
                .DeepEqual(expectedOutputJson.AsJson())
                .Should().BeTrue();
        }

        private static IEnumerable<TestItem> GetTestData()
            => Enumerable.Empty<TestItem>()
                .Concat(SimplePrograms())
                .Concat(AssignmentPrograms());

        private static IEnumerable<TestItem> SimplePrograms()
        {
            yield return new TestItem("", "null", "[null]");

            yield return new TestItem(".", "null", "[null]");

            yield return new TestItem(". + 1", "1", "[2]");

            yield return new TestItem(". + 1 - 2", "1", "[0]");

            yield return new TestItem(".[]", "[1, 2]", "[1, 2]");

            yield return new TestItem(".[] + 2", "[1, 2]", "[3, 4]");

            yield return new TestItem(".[1] + 2", "[1, 2]", "[4]");

            yield return new TestItem("1 + .[1] * 2", "[1, 2]", "[5]");

            yield return new TestItem("(1 + .[1]) * 2", "[1, 2]", "[6]");

            yield return new TestItem(". | .", "[1, 2]", "[[1, 2]]");

            yield return new TestItem(".[1] | (1 + .) * 2", "[1, 2]", "[6]");

            yield return new TestItem(".a | . + 2", "{ \"a\": 1, \"b\": 2}", "[3]");

            yield return new TestItem(
                ".a.b | . + 2",
                "{ \"a\": { \"b\": 1, \"c\": 2 }, \"d\": 2}",
                "[3]");
        }

        private static IEnumerable<TestItem> AssignmentPrograms()
        {
            yield return new TestItem(
                ".[][] |= . + 2",
                "[[1, 2], [3, 4]]",
                "[[[3, 4], [5, 6]]]");
        }
    }
}