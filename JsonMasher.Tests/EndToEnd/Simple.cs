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
                .Concat(AssignmentPrograms())
                .Concat(IfThenElsePrograms())
                .Concat(BindingPrograms());

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

            yield return new TestItem(
                ".a.b as $test | $test",
                "{ \"a\": { \"b\": 1, \"c\": 2 }, \"d\": 2}",
                "[1]");

            yield return new TestItem(
                ".a.b as $test | $test | $test + 2",
                "{ \"a\": { \"b\": 1, \"c\": 2 }, \"d\": 2}",
                "[3]");

            yield return new TestItem(
                "{a:1}",
                "null",
                "[{\"a\":1}]");

            yield return new TestItem(
                ".[] | {a:.}",
                "[1, 2]",
                "[{\"a\":1}, {\"a\":2}]");

            yield return new TestItem(
                "{a:., b:.[]}",
                "[1, 2]",
                "[{\"a\": [1, 2], \"b\": 1}, {\"a\": [1, 2], \"b\": 2}]");

            yield return new TestItem(
                ".\"a\"",
                "{ \"a\": 1 }",
                "[1]");

            yield return new TestItem("./2", "1", "[0.5]");

            yield return new TestItem(". > 3", "1", "[false]");
            yield return new TestItem(".[] | . <= 2", "[1, 2]", "[true, true]");
            yield return new TestItem(".[] | . > 2", "[1, 2, 3, 4]", "[false, false, true, true]");
            yield return new TestItem(".[] | . >= 2", "[1, 2, 3, 4]", "[false, true, true, true]");

            yield return new TestItem(". > 3 and . < 4", "3.5", "[true]");
            yield return new TestItem(". > 3 and . < 4", "3", "[false]");
            yield return new TestItem(". < 3 or (. < 4 | not)", "3.5", "[false]");
            yield return new TestItem(". < 3 or (. < 4 | not)", "2", "[true]");
            yield return new TestItem(". < 3 or (. < 4 | not)", "5", "[true]");

            yield return new TestItem(". > 3 and . < 5 or . > 7 and . < 9", "4", "[true]");
            yield return new TestItem(". > 3 and . < 5 or . > 7 and . < 9", "8", "[true]");
            yield return new TestItem(". > 3 and . < 5 or . > 7 and . < 9", "6", "[false]");

            yield return new TestItem("empty", "null", "[]");
            yield return new TestItem(". | empty", "[1, 2, 3]", "[]");
            yield return new TestItem("empty | .", "[1, 2, 3]", "[]");
        }

        private static IEnumerable<TestItem> IfThenElsePrograms()
        {
            yield return new TestItem("if true, false then 1 else 2 end", "null", "[1, 2]");
            yield return new TestItem("if . < 3 then 1 else 2 end", "2", "[1]");
            yield return new TestItem("if . == 4 then 1 else 2 end", "2", "[2]");
            yield return new TestItem(
                "if . == 4 then 1 elif . == 2 then 3 else 2 end", "2", "[3]");
        }

        private static IEnumerable<TestItem> AssignmentPrograms()
        {
            yield return new TestItem(
                ".[][] |= . + 2",
                "[[1, 2], [3, 4]]",
                "[[[3, 4], [5, 6]]]");
            yield return new TestItem(
                ".[][][] |= . + 2",
                "[[[1, 2], [3, 4]]]",
                "[[[[3, 4], [5, 6]]]]");
            yield return new TestItem(
                ".[0] |= . + 2",
                "[1, 2, 3, 4]",
                "[[3, 2, 3, 4]]");
            yield return new TestItem(
                ".[].a.b |= . + 2",
                @"[{ ""a"": { ""b"": 1}, ""c"": 2 }, { ""a"": { ""b"": 3}, ""c"": 4 }]",
                @"[[{ ""a"": { ""b"": 3}, ""c"": 2 }, { ""a"": { ""b"": 5}, ""c"": 4 }]]");
            yield return new TestItem(
                ".[].a[1] |= . + 2",
                @"[{ ""a"": [1, 2] }, { ""a"": [3, 4] }]",
                @"[[{ ""a"": [1, 4] }, { ""a"": [3, 6] }]]");
            yield return new TestItem(
                ".[0,1] |= . + 2",
                "[1, 2, 3, 4]",
                "[[3, 4, 3, 4]]");
            yield return new TestItem(
                "(.[0] |= . + 2 | .[0]), .[0]",
                "[1, 2]",
                "[3, 1]");
        }

        private static IEnumerable<TestItem> BindingPrograms()
        {
            yield return new TestItem(
                "1, 2 as $test | $test",
                "null",
                "[1, 2]");
        }
    }
}
