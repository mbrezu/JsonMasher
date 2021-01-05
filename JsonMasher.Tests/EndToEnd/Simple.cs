using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Compiler;
using JsonMasher.Mashers;
using Xunit;

namespace JsonMasher.Tests.EndToEnd
{
    public class Simple
    {
        private record TestItem(
            string Program,
            string InputJson,
            string ExpectedOutputJson,
            string ExpectedStdErrJson = null);

        public static IEnumerable<object[]> TestData
            => GetTestData().Select((System.Func<TestItem, object[]>)(item => (new object[] {
                item.Program,
                item.InputJson,
                item.ExpectedOutputJson,
                item.ExpectedStdErrJson
            })));

        [Theory]
        [MemberData(nameof(TestData))]
        public void ProgramTest(
            string program, string inputJson, string expectedOutputJson, string expectedStdErrJson)
        {
            // Arrange
            var parser = new Parser();
            var (filter, _) = parser.Parse(program);
            var input = inputJson.AsJson().AsEnumerable();

            // Act
            var result = new Mashers.JsonMasher().Mash(input, filter, DefaultMashStack.Instance);

            // Assert
            Json.Array(result.sequence)
                .DeepEqual(expectedOutputJson.AsJson())
                .Should().BeTrue();
            if (expectedStdErrJson != null)
            {
                Json.Array(result.context.Log)
                    .DeepEqual(expectedStdErrJson.AsJson())
                    .Should().BeTrue();
            }
        }

        private static IEnumerable<TestItem> GetTestData()
            => Enumerable.Empty<TestItem>()
                .Concat(SimplePrograms())
                .Concat(AssignmentPrograms())
                .Concat(PathTests())
                .Concat(IfThenElsePrograms())
                .Concat(BindingPrograms())
                .Concat(ProgramsWithFunctions())
                .Concat(StandardLibrary());

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

            yield return new TestItem("range(3)", "null", "[0, 1, 2]");
            yield return new TestItem("range(2, 3)", "null", "[0, 1, 0, 1, 2]");
            yield return new TestItem("range(range(3))", "null", "[0, 0, 1]");
            yield return new TestItem("range(1; 3)", "null", "[1, 2]");
            yield return new TestItem(
                "range(2,3; 5,6)", "null", "[2, 3, 4, 2, 3, 4, 5, 3, 4, 3, 4, 5]");
            yield return new TestItem(
                "range(1; 6; 2)", "null", "[1, 3, 5]");
            yield return new TestItem(
                "range(1; 3; 0.5)", "null", "[1, 1.5, 2, 2.5]");
            yield return new TestItem(
                "range(1; 3,4; 0.5)", "null", "[1, 1.5, 2, 2.5, 1, 1.5, 2, 2.5, 3, 3.5]");
            yield return new TestItem(
                "range(1,2; 2,3; 1,0.5)", "null", "[1, 1, 1.5, 1, 2, 1, 1.5, 2, 2.5, 2, 2, 2.5]");
            yield return new TestItem(
                "range(-2; 0)", "null", "[-2, -1]");

            yield return new TestItem("length", "null", "[1]");
            yield return new TestItem("length", "true", "[1]");
            yield return new TestItem("length", "false", "[1]");
            yield return new TestItem("length", "100", "[1]");
            yield return new TestItem("length", "\"test\"", "[4]");
            yield return new TestItem("length", "[1, 2, 3]", "[3]");
            yield return new TestItem("{a: 1, b: 2, c: 3} | length", "null", "[3]");

            yield return new TestItem("limit(2; .[])", "[1, 2, 3]", "[1, 2]");
            yield return new TestItem("limit(1, 2, 3; .[])", "[1, 2, 3]", "[1, 1, 2, 1, 2, 3]");

            yield return new TestItem("-.2", "null", "[-0.2]");
            yield return new TestItem("1 + -2", "null", "[-1]");
            yield return new TestItem("-(1 + -2)", "null", "[1]");
            yield return new TestItem(".[-1]", "[1, 2, 3]", "[3]");
            yield return new TestItem(".[-2]", "[1, 2, 3]", "[2]");
            yield return new TestItem(".[range(-2; 0)]", "[1, 2, 3]", "[2, 3]");

            yield return new TestItem(".[10]", "[1, 2, 3]", "[null]");
            yield return new TestItem(".[-10]", "[1, 2, 3]", "[null]");
            yield return new TestItem(".a", "{ \"b\": 2 }", "[null]");

            yield return new TestItem("keys", "{ \"b\": 2 }", "[[\"b\"]]");
            yield return new TestItem("keys", "{ \"b\": 2, \"a\": 1 }", "[[\"a\", \"b\"]]");
            yield return new TestItem("keys", "[true, true]", "[[0, 1]]");

            yield return new TestItem(".a?", "100", "[]");
            yield return new TestItem(".\"a\"?", "100", "[]");
            yield return new TestItem(".[0]?", "100", "[]");

            yield return new TestItem(".[1:]", "[1, 2, 3]", "[[2, 3]]");
            yield return new TestItem(".[1:]?", "0", "[]");
            yield return new TestItem(".[]?", "0", "[]");
            yield return new TestItem(".[:-1]", "[1, 2, 3]", "[[1, 2]]");
            yield return new TestItem(".[:-1]?", "0", "[]");
            yield return new TestItem(".[-2:-1]", "[1, 2, 3]", "[[2]]");
            yield return new TestItem(".[-2:-1]?", "0", "[]");
            yield return new TestItem(".[-2:]", "[1, 2, 3]", "[[2,3]]");
            yield return new TestItem(".[:2]", "[1, 2, 3]", "[[1,2]]");
            yield return new TestItem(".[:1,2]", "[1, 2, 3]", "[[1], [1,2]]");
            yield return new TestItem(".[1,2:2,3]", "[1, 2, 3, 4]", "[[2], [2, 3], [], [3]]");
            yield return new TestItem(".[1,2:]", "[1, 2, 3, 4]", "[[2, 3, 4], [3, 4]]");
            yield return new TestItem(".[-2:1]", "[1, 2, 3, 4, 5]", "[[]]");

            yield return new TestItem("empty // 2", "null", "[2]");
            yield return new TestItem("(empty, false, null) // 2", "null", "[2]");
            yield return new TestItem("(empty, false, 1) // 2", "null", "[1]");

            yield return new TestItem("def foo: .a.b; {a: {b: {c: 1}}} | foo.c", "null", "[1]");
            yield return new TestItem("{a: 1 | 2}", "null", "[{\"a\": 2}]");
            yield return new TestItem("[1 | 2, 3 | 4]", "null", "[[4, 4]]");

            yield return new TestItem("0 | (1, 2, .[], 3, 4)?", "null", "[1, 2]");

            yield return new TestItem("\"a\" * 0", "null", "[null]");
            yield return new TestItem("0 * \"a\"", "null", "[null]");

            yield return new TestItem(
                "{\"a\": { \"b\": 1 }, \"c\": 2 } * .",
                "{\"a\": { \"b\": 2, \"d\": 3 }, \"c\": 4 }",
                "[{ \"a\": { \"b\": 2, \"d\": 3 }, \"c\": 4 }]");

            yield return new TestItem("\"a b c\" / \" \"", "null", "[[\"a\", \"b\", \"c\"]]");

            yield return new TestItem(
                "0 | try (1, 2, .[], 3, 4)",
                "null",
                "[1, 2]");
            yield return new TestItem(
                "0 | try (1, 2, .[], 3, 4) catch .",
                "null",
                "[1, 2, \"Can't enumerate Number.\"]");
            yield return new TestItem(
                "0 | try (1, 2, .[], 3, 4) catch . * 2",
                "null",
                "[1, 2, \"Can't enumerate Number.Can't enumerate Number.\"]");
        }

        private static IEnumerable<TestItem> IfThenElsePrograms()
        {
            yield return new TestItem("if true,  false then 1 else 2 end", "null", "[1, 2]");
            yield return new TestItem("if . < 3 then 1 else 2 end", "2", "[1]");
            yield return new TestItem("if . == 4 then 1 else 2 end", "2", "[2]");
            yield return new TestItem("if . != 4 then 1 else 2 end", "2", "[1]");
            yield return new TestItem(
                "if . == 4 then 1 elif . == 2 then 3 else 2 end", "2", "[3]");
            yield return new TestItem(
                "if empty then 1 else 2 end", "2", "[]");
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
            yield return new TestItem(
                "def a_then_b: .a.b; a_then_b.c |= . + 3",
                "{\"a\":{\"b\":{\"c\": 4}}}",
                "[{\"a\":{\"b\":{\"c\": 7}}}]");
            yield return new TestItem(
                "def f_then_b(f): .[f].b; f_then_b(\"a\").c |= . + 3",
                "{\"a\":{\"b\":{\"c\": 4}}}",
                "[{\"a\":{\"b\":{\"c\": 7}}}]");
            yield return new TestItem(
                "empty |= . + 2",
                "[1, 2, 3]",
                "[[1, 2, 3]]");
            yield return new TestItem(
                "(.[] | select(. < 3)) |= . + 2",
                "[1, 2, 3]",
                "[[3, 4, 3]]");
            yield return new TestItem(
                ".[1:] |= [7] + .",
                "[1, 2, 3]",
                "[[1, 7, 2, 3]]");
            yield return new TestItem(
                "(1 as $test | .[2]) |= 5",
                "[1, 2, 3]",
                "[[1, 2, 5]]");
            yield return new TestItem(
                "((1, 2) as $test | .[$test]) |= 5",
                "[1, 2, 3]",
                "[[1, 5, 5]]");
        }

        private static IEnumerable<TestItem> PathTests()
        {
            yield return new TestItem("path(.)", "[1, 2, 3]", "[[]]");
            yield return new TestItem("path(.a)", "null", "[[\"a\"]]");
            yield return new TestItem("path(.\"a\")", "null", "[[\"a\"]]");
            yield return new TestItem("path(.a.b)", "null", "[[\"a\", \"b\"]]");
            yield return new TestItem("path(.a.b.c)", "null", "[[\"a\", \"b\", \"c\"]]");
            yield return new TestItem("path(.[]?)", "null", "[]");
            yield return new TestItem("path(.[])", "[1, 2, 3]", "[[0], [1], [2]]");
            yield return new TestItem("path(.[])", "{\"a\":1,\"b\":2}", "[[\"a\"], [\"b\"]]");
            yield return new TestItem("path(.[][]?)", "{\"a\":[1, 2],\"b\":2}", "[[\"a\", 0], [\"a\", 1]]");
            yield return new TestItem("path(.[0])", "null", "[[0]]");
            yield return new TestItem("path(.[0, 1, 2])", "null", "[[0], [1], [2]]");
            yield return new TestItem(
                "path(.[0, 1, 2][3, 4])",
                "null",
                "[[0, 3], [0, 4], [1, 3], [1, 4], [2, 3], [2, 4]]");
            yield return new TestItem(
                "def a_then_b: .a.b; path(a_then_b.c)",
                "null",
                "[[\"a\", \"b\", \"c\"]]");
            yield return new TestItem(
                "def a_then_b(a): a.b; path(a_then_b(.a).c)",
                "null",
                "[[\"a\", \"b\", \"c\"]]");
            yield return new TestItem(
                "path(if 1 then .a else .b end)",
                "null",
                "[[\"a\"]]");
            yield return new TestItem(
                "path(if true, false then .a else .b end)",
                "null",
                "[[\"a\"], [\"b\"]]");
            yield return new TestItem(
                "path(empty)",
                "null",
                "[]");
            yield return new TestItem(
                "path(.[] | select(. < 3))",
                "[1, 2, 3, 4]",
                "[[0], [1]]");
            yield return new TestItem(
                "path(.a, .b)",
                "null",
                "[[\"a\"], [\"b\"]]");
            yield return new TestItem(
                "path(.a, .b | .[0])",
                "null",
                "[[\"a\", 0], [\"b\", 0]]");
            yield return new TestItem(
                "path(.[10:15])",
                "null",
                "[[{\"start\": 10, \"end\": 15}]]");
            yield return new TestItem(
                "path(.[10:])",
                "null",
                "[]");
            yield return new TestItem(
                "path(..)",
                "null",
                "[[]]");
            yield return new TestItem(
                "path(..)",
                "[1, 2, 3]",
                "[[], [0], [1], [2]]");
            yield return new TestItem(
                "path(..)",
                "{\"a\":1,\"b\":2}",
                "[[], [\"a\"], [\"b\"]]");
            yield return new TestItem(
                "path((1, 2, 3) as $test | .[$test])",
                "null",
                "[[1], [2], [3]]");
        }

        private static IEnumerable<TestItem> BindingPrograms()
        {
            yield return new TestItem(
                "1, 2 as $test | $test",
                "null",
                "[1, 2]");
        }

        private static IEnumerable<TestItem> ProgramsWithFunctions()
        {
            yield return new TestItem(
                "def point(x; y): [x, y]; point(1; 2)",
                "null",
                "[[1, 2]]");
            yield return new TestItem(
                "def map(x): [.[] | x]; map(. + 2)",
                "[1, 2, 3]",
                "[[3, 4, 5]]");
            yield return new TestItem(
                @"
def map(x): [.[] | x];
def select(x): if x then . else empty end;
map(select(. < 2))",
                "[1, 2, 3]",
                "[[1]]");
            yield return new TestItem(
                @"
def map(x): def result: [.[] | x]; result;
def select(x): if x then . else empty end;
map(select(. < 2))",
                "[1, 2, 3]",
                "[[1]]");
        }

        private static IEnumerable<TestItem> StandardLibrary()
        {
            yield return new TestItem("map(. + 2)", "[1, 2, 3]", "[[3, 4, 5]]");
            yield return new TestItem("map(select(. > 5))", "[1, 2, 3]", "[[]]");
            yield return new TestItem("debug", "[1, 2, 3]", "[[1, 2, 3]]", "[[\"DEBUG\", [1, 2, 3]]]");
            yield return new TestItem("..", "[1, 2, 3]", "[[1, 2, 3], 1, 2, 3]");
            yield return new TestItem("recurse", "[1, 2, 3]", "[[1, 2, 3], 1, 2, 3]");
            yield return new TestItem("map_values(. + 1)", "[1, 2, 3]", "[[2, 3, 4]]");
            yield return new TestItem(
                "{a:1, b:2} | map_values(. + 1)", "null", "[{ \"a\": 2, \"b\": 3}]");
            yield return new TestItem(
                "{a:1, b:2} | keys_unsorted", "null", "[[\"a\", \"b\"]]");
            yield return new TestItem("[1, 2, 0, true] | sort", "null", "[[true, 0, 1, 2]]");
            yield return new TestItem(
                "[{ a: 1, b: 2}, {a: -1, b: 3}, {a: -2, b: 5}] | sort_by(.a)",
                "null",
                "[[{\"a\": -2, \"b\": 5}, {\"a\": -1, \"b\": 3}, { \"a\": 1, \"b\": 2}]]");
        }
    }
}
