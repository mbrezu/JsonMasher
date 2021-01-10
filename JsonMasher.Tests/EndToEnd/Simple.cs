using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;
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
                .Concat(ReduceAndForeach())
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
                "{(\"a\", \"b\"):(1, 2)}",
                "null",
                "[{\"a\":1}, {\"a\":2}, {\"b\":1}, {\"b\":2}]");

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
            yield return new TestItem(".[2:8]", "[1, 2, 3]", "[[3]]");

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
            yield return new TestItem("7 % 2", "null" , "[1]");

            yield return new TestItem("has(0,1,7)", "[1, 2, 3]" , "[true, true, false]");
            yield return new TestItem(
                "has(\"a\", \"b\", \"c\")",
                "{\"a\":1,\"b\":2}",
                "[true, true, false]");
            yield return new TestItem(".[] | in([1, 2, 3])", "[1, 7]" , "[true, false]");
            yield return new TestItem(
                ".[] | in({a:1,b:2})",
                "[\"a\", \"c\"]",
                "[true, false]");
            yield return new TestItem(
                ". as $array | $array[1]",
                "[1, 2, 3]",
                "[2]");
            yield return new TestItem(
                ". as $array | 1 | $array[.]",
                "[1, 2, 3]",
                "[2]");
            yield return new TestItem(
                ". as $array | $array[1:]",
                "[1, 2, 3]",
                "[[2, 3]]");
            yield return new TestItem(
                ". as $array | 1 | $array[.:]",
                "[1, 2, 3]",
                "[[2, 3]]");
            yield return new TestItem(
                ". as $object | $object[\"b\"]",
                "{\"a\": 1, \"b\": 2}",
                "[2]");
            yield return new TestItem(
                ". as $object | \"b\" | $object[\"b\"]",
                "{\"a\": 1, \"b\": 2}",
                "[2]");
            
            yield return new TestItem(
                ".[[2]]",
                "[1, 2, 3, 4, 2]",
                "[[1, 4]]");
            yield return new TestItem(
                ".[[2, 3]]",
                "[1, 2, 3, 4, 2]",
                "[[1]]");
            yield return new TestItem(
                ".[[2, 3]]",
                "[1, 2, 3, 4, 2, 2, 3]",
                "[[1, 5]]");
            yield return new TestItem(
                ".[[2, 3, 4], [2, 3]]",
                "[1, 2, 3, 4, 2, 3, 2, 3, 4]",
                "[[1,6],[1,4,6]]");
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
                ".[][] = .[1][0] + 2",
                "[[1, 2], [3, 4]]",
                "[[[5, 5], [5, 5]]]");
            yield return new TestItem(
                ".[][][] |= . + 2",
                "[[[1, 2], [3, 4]]]",
                "[[[[3, 4], [5, 6]]]]");
            yield return new TestItem(
                ".[0] |= . + 2",
                "[1, 2, 3, 4]",
                "[[3, 2, 3, 4]]");
            yield return new TestItem(
                ".[0] = .[1] + 2",
                "[1, 2, 3]",
                "[[4, 2, 3]]");
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
            yield return new TestItem(
                "(.[5] // .[1]) |= 5",
                "[1, 2, 3]",
                "[[1, 5, 3]]");
            yield return new TestItem(
                "[1, 2, 3] | .[0, 1] = (100, 200)",
                "null",
                "[[100, 100, 3], [200, 200, 3]]");
            yield return new TestItem(
                "{} | .[\"a\"] = 1",
                "null",
                "[{\"a\": 1}]");
            yield return new TestItem(
                "{} | .a.b = 1",
                "null",
                "[{\"a\": {\"b\": 1}}]");
            yield return new TestItem(
                "{a: {c: 2}} | .a.b = 1",
                "null",
                "[{\"a\": {\"b\": 1, \"c\": 2}}]");
            yield return new TestItem(
                "{} | .a[5] = 1",
                "null",
                "[{\"a\": [null, null, null, null, null, 1]}]");
            yield return new TestItem(
                "{a: [0, 1]} | .a[5] = 1",
                "null",
                "[{\"a\": [0, 1, null, null, null, 1]}]");
            yield return new TestItem(
                "{a: [0]} | .a[100:] = [1]",
                "null",
                "[{\"a\": [0, 1]}]");
            yield return new TestItem(
                "{} | .a[100:] = [1]",
                "null",
                "[{\"a\": [1]}]");
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
                "[[{\"end\": 0, \"start\": 10}]]");
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
            yield return new TestItem(
                "path(.[2] // .[1])",
                "[1, 2, 3]",
                "[[2]]");
            yield return new TestItem(
                "path(.[2] // .[1])",
                "null",
                "[[1]]");
            yield return new TestItem(
                "[{a:1, b:2}, {a:3, b:4}, {a:5, b:6}] | path(.[-2:][].a)",
                "null",
                "[[{\"end\": 3, \"start\": 1}, 0, \"a\"], [{\"end\": 3, \"start\": 1}, 1, \"a\"]]");
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
            yield return new TestItem("def f(x): x; def g(x): f(x); g(1)", "null", "[1]");
        }

        private static IEnumerable<TestItem> ReduceAndForeach()
        {
            yield return new TestItem(
                "reduce .[] as $item (0; . + $item)", "[1, 2, 3]", "[6]");
            yield return new TestItem(
                "foreach .[] as $item (0; . + $item)", "[1, 2, 3]", "[1, 3, 6]");
            yield return new TestItem(
                "foreach .[] as $item (0; . + $item; . * .)", "[1, 2, 3]", "[1, 9, 36]");
            yield return new TestItem(
                "foreach .[] as $item (0; . + $item; range(.))",
                "[1, 2, 3]",
                "[0, 0, 1, 2, 0, 1, 2, 3, 4, 5]");
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

            yield return new TestItem("[[1, 2], 3] | getpath([0, 1])", "null", "[2]");
            yield return new TestItem("[[1, 2], 3] | getpath([])", "null", "[[[1, 2], 3]]");
            yield return new TestItem("[[1, 2], 3] | [getpath(path(..))] == [..]", "null", "[true]");

            yield return new TestItem(
                "[1, 2, 3] | setpath(path(.[2,1]); 100, 200)",
                "null",
                "[[1,2,100],[1,100,3],[1,2,200],[1,200,3]]");

            yield return new TestItem(
                ".[] | delpaths([path(.[2,1])], [path(.[.0])])",
                "[[1, 2, 3], [3, 4, 5]]",
                "[[1],[2,3],[3],[4,5]]");

            yield return new TestItem(
                "to_entries",
                "{\"a\": 1, \"b\": 2}",
                "[[{\"key\": \"a\", \"value\": 1}, {\"key\": \"b\", \"value\": 2}]]");

            yield return new TestItem("add", "[1, 2, 3]", "[6]");
            yield return new TestItem(
                "add",
                "[[1], [2, 3]]",
                "[[1, 2, 3]]");

            yield return new TestItem(
                "to_entries | from_entries",
                "{\"a\": 1, \"b\": 2}",
                "[{\"a\": 1, \"b\": 2}]");
            yield return new TestItem(
                "with_entries(.value |= . * .)",
                "{\"a\": 1, \"b\": 2}",
                "[{\"a\": 1, \"b\": 4}]");

            yield return new TestItem(
                "{ a: 1, b: [1, 2, 3] } | del(.b[1])",
                "null",
                "[{ \"a\": 1, \"b\": [1, 3]}]");
            yield return new TestItem(
                "{ a: 1, b: [1, 2, 3] } | del(.b[1,2])",
                "null",
                "[{ \"a\": 1, \"b\": [1]}]");
            yield return new TestItem(
                "{ a: 1, b: [1, 2, 3] } | del(.b[-2:])",
                "null",
                "[{ \"a\": 1, \"b\": [1]}]");
            yield return new TestItem(
                "[{a:1, b:2}, {a:3, b:4}, {a:5, b:6}] | del(.[-2:][].a)",
                "null",
                "[[{\"a\":1,\"b\": 2},{\"b\": 4},{\"b\": 6}]]");

            yield return new TestItem("..", "1", "[1]");
            yield return new TestItem("..", "\"a\"", "[\"a\"]");
            yield return new TestItem("..", "true", "[true]");
            yield return new TestItem("..", "false", "[false]");
            yield return new TestItem("..", "null", "[null]");
            yield return new TestItem("..", "[1,2]", "[[1, 2], 1, 2]");
            yield return new TestItem(
                "..",
                "{\"a\": 1, \"b\": 2}", 
                "[{\"a\": 1, \"b\": 2}, 1, 2]");
            yield return new TestItem(
                "..",
                "{\"a\": 1, \"b\": [1, 2]}", 
                "[{\"a\": 1, \"b\": [1, 2]}, 1, [1, 2], 1, 2]");
            yield return new TestItem(
                "type",
                "{\"a\": 1, \"b\": [1, 2]}", 
                "[\"object\"]");
            yield return new TestItem(
                ".[] | arrays", "[1, [1], [1, 2], \"test\"]", "[[1], [1, 2]]");
            yield return new TestItem(
                ".[] | objects", "[1, {}, {\"a\":1}, \"test\"]", "[{}, {\"a\":1}]");
            yield return new TestItem(
                ".[] | iterables",
                "[1, [], [1, 2], {}, {\"a\":1}, \"test\"]",
                "[[], [1, 2], {}, {\"a\":1}]");
            yield return new TestItem(
                ".[] | numbers",
                "[1, [], 2, {}, {\"a\":1}, \"test\"]",
                "[1, 2]");
            yield return new TestItem(
                ".[] | strings",
                "[1, [], 2, {}, {\"a\":1}, \"test\"]",
                "[\"test\"]");
            yield return new TestItem(
                ".[] | nulls",
                "[1, [], 2, null, {}, null, {\"a\":1}, \"test\"]",
                "[null, null]");
            yield return new TestItem(
                ".[] | values",
                "[1, [], 2, null, {}, null, {\"a\":1}, \"test\"]",
                "[1, [], 2, {}, {\"a\":1}, \"test\"]");
            yield return new TestItem(
                ".[] | scalars",
                "[1, [], 2, null, {}, null, {\"a\":1}, \"test\"]",
                "[1, 2, null, null, \"test\"]");
            yield return new TestItem(
                "reverse",
                "[1, 2, 3]",
                "[[3, 2, 1]]");
            yield return new TestItem(
                "paths",
                "[1, 2, 3]",
                "[[0], [1], [2]]");
            yield return new TestItem(
                "paths(. < 3)",
                "[1, 2, 3]",
                "[[0], [1]]");
            yield return new TestItem(
                "paths(. < 3)",
                "{\"a\": 1, \"b\": 2, \"c\": [1, 2, 3]}",
                "[[\"a\"],[\"b\"],[\"c\",0],[\"c\",1]]");
            yield return new TestItem(
                "isinfinite",
                "100",
                "[false]");
            yield return new TestItem(
                "isfinite",
                "100",
                "[true]");
            yield return new TestItem(
                ".[] | finites",
                "[100, 200, 300]",
                "[100, 200, 300]");
            yield return new TestItem(
                "isnormal",
                "100",
                "[true]");
            yield return new TestItem(
                ".[] | normals",
                "[-100, 0, 100, 200, 300]",
                "[-100, 100, 200, 300]");
            yield return new TestItem(
                "leaf_paths",
                "[-100, 0, [1, 2, 3, {\"a\": 1}], 100, 200, 300]",
                "[[0],[1],[2, 0],[2, 1],[2, 2],[2, 3, \"a\"],[3],[4],[5]]");
            yield return new TestItem(
                "tostring",
                "[null, false, true, 10]",
                "[\"[null, false, true, 10]\"]");
            yield return new TestItem(
                "tonumber",
                "\"102\"",
                "[102]");
            yield return new TestItem(
                "join(\" \")",
                "[\"102\", 103, 104]",
                "[\"102 103 104\"]");
            yield return new TestItem(
                "flatten",
                "[[1, [2, 3]], 4]",
                "[[1, 2, 3, 4]]");
            yield return new TestItem(
                "flatten(1)",
                "[[1, [2, 3]], 4]",
                "[[1, [2, 3], 4]]");
            yield return new TestItem(
                "try \"not good\" | error catch .",
                "null",
                "[\"not good\"]");
            yield return new TestItem(
                "try error(\"not good\") catch .",
                "null",
                "[\"not good\"]");

            yield return new TestItem("first(.[])", "[1, 2, 3]", "[1]");
            yield return new TestItem("first", "[1, 2, 3]", "[1]");

            yield return new TestItem("last(.[])", "[1, 2, 3]", "[3]");
            yield return new TestItem("last", "[1, 2, 3]", "[3]");

            yield return new TestItem("nth(1; .[])", "[1, 2, 3]", "[2]");
            yield return new TestItem("nth(1)", "[1, 2, 3]", "[2]");

            yield return new TestItem("isempty(1)", "[1, 2, 3]", "[false]");
            yield return new TestItem("isempty(.[])", "[1, 2, 3]", "[false]");
            yield return new TestItem("isempty(.[] | strings)", "[1, 2, 3]", "[true]");

            yield return new TestItem("all", "[1, 2, 3]", "[true]");
            yield return new TestItem("all(. > 0)", "[1, 2, 3]", "[true]");
            yield return new TestItem("all(.[]; . > 0)", "[1, 2, 3]", "[true]");
            yield return new TestItem("any", "[false, 2, false]", "[true]");
            yield return new TestItem("any(. > 0)", "[-1, 2, -3]", "[true]");
            yield return new TestItem("any(.[]; . > 0)", "[-1, 2, -3]", "[true]");

            yield return new TestItem(
                "combinations",
                "[[1, 2], [3, 4]]",
                "[[1,3],[1,4],[2,3],[2,4]]");
            yield return new TestItem(
                "combinations(2)",
                "[0, 1]",
                "[[0,0],[0,1],[1,0],[1,1]]");

            yield return new TestItem(
                "group_by(.)",
                "[0,1,0,2]",
                "[[[0, 0], [1], [2]]]");

            yield return new TestItem(
                "unique",
                "[0,1,0,2]",
                "[[0,1,2]]");
            yield return new TestItem(
                "unique_by(.)",
                "[0,1,0,2]",
                "[[0,1,2]]");

            yield return new TestItem(
                "min",
                "[0,1,0,2]",
                "[0]");
            yield return new TestItem(
                "min",
                "[]",
                "[null]");

            yield return new TestItem(
                "min_by(.)",
                "[0,1,0,2]",
                "[0]");
            yield return new TestItem(
                "min_by(.)",
                "[]",
                "[null]");

            yield return new TestItem(
                "max",
                "[0,1,0,2]",
                "[2]");
            yield return new TestItem(
                "max",
                "[]",
                "[null]");
            yield return new TestItem(
                "max_by(.)",
                "[0,1,0,2]",
                "[2]");
            yield return new TestItem(
                "max_by(.)",
                "[]",
                "[null]");

            yield return new TestItem(
                "indices([2])",
                "[1, 2, 3]",
                "[[1]]");
            yield return new TestItem(
                "indices(2)",
                "[1, 2, 3]",
                "[[1]]");
            yield return new TestItem(
                "index(2)",
                "[1, 2, 3, 2]",
                "[1]");
            yield return new TestItem(
                "rindex(2)",
                "[1, 2, 3, 2]",
                "[3]");
            yield return new TestItem(
                "indices(\"acab\")",
                "\"abacabacab\"",
                "[[2, 6]]");
            yield return new TestItem(
                "indices(\"acab\", \"ac\")",
                "\"abacabacab\"",
                "[[2, 6], [2, 6]]");
            yield return new TestItem(
                "index(\"acab\", \"ac\")",
                "\"abacabacab\"",
                "[2, 2]");
            yield return new TestItem(
                "rindex(\"acab\", \"ac\")",
                "\"abacabacab\"",
                "[6, 6]");

            yield return new TestItem(
                "transpose",
                "[[1, 2], [3, 4]]",
                "[[[1, 3], [2, 4]]]");
            yield return new TestItem(
                "transpose",
                "[[1, 2], [3, 4], [5, 6, 7]]",
                "[[[1, 3, 5], [2, 4, 6], [null, null, 7]]]");

            yield return new TestItem("floor", "10.50", "[10]");

            yield return new TestItem("sqrt", "100", "[10]");

            yield return new TestItem("until(. == 3; . + 1)", "1", "[3]");

            yield return new TestItem("while(. < 4; . + 1)", "1", "[1, 2, 3]");

            yield return new TestItem(
                "bsearch(2)",
                "[1, 2, 3, 4, 5]",
                "[1]");

            yield return new TestItem("fabs", "-100", "[100]");

            yield return new TestItem("round", "11.5", "[12]");
            yield return new TestItem("round", "11.45", "[11]");

            yield return new TestItem("ceil", "11.45", "[12]");

            yield return new TestItem("trunc", "11.45", "[11]");
            yield return new TestItem("trunc", "-11.45", "[-11]");

            yield return new TestItem("pow(2; 10)", "null", "[1024]");
            yield return new TestItem("pow(10; 2)", "null", "[100]");
            yield return new TestItem("pow(10; 2, 3)", "null", "[100, 1000]");
            yield return new TestItem("pow(3, 4; 2, 3)", "null", "[9, 27, 16, 64]");

            yield return new TestItem("limit(3; repeat(1))", "null", "[1, 1, 1]");

            yield return new TestItem(
                "contains([1], [1, 3], [4])", "[1, 2, 3]", "[true, true, false]");

            yield return new TestItem(
                "{a:1, c:3}, {a:1}, {a:2} | inside({a:1, b:2, c:3})", "null", "[true, true, false]");
            yield return new TestItem(
                "\"ab\", \"abc\", \"ad\" | inside(\"abcd\")", "null", "[true, true, false]");

            yield return new TestItem(
                "\"abcd\" | startswith(\"ab\", \"abc\", \"ac\")", "null", "[true, true, false]");

            yield return new TestItem(
                "\"abcd\" | endswith(\"bcd\", \"cd\", \"ac\")", "null", "[true, true, false]");

            yield return new TestItem(
                "\"abcd\" | utf8bytelength", "null", "[4]");

            yield return new TestItem(
                "\"abcd\" | explode", "null", "[[97, 98, 99, 100]]");

            yield return new TestItem(
                "\"abcd\" | explode | implode", "null", "[\"abcd\"]");

            yield return new TestItem(
                "\"    abcd\" | ltrimstr(\" \")", "null", "[\"abcd\"]");

            yield return new TestItem(
                "\"abcd    \" | rtrimstr(\" \")", "null", "[\"abcd\"]");
        }
    }
}
