using System.Collections.Generic;
using System.Linq;
using Shouldly;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Primitives;
using Xunit;

namespace JsonMasher.Tests.Builtins
{
    public class Misc
    {
        private record TestItem(IJsonMasherOperator Op, string Input, string Output, string StdErr = null);

        [Theory]
        [MemberData(nameof(TestData))]
        public void MiscellaneousTestsTheory(
            IJsonMasherOperator op, string input, string output, string stdErr)
        {
            // Arrange

            // Act
            var (result, context) = op.RunAsSequenceWithContext(input.AsJson());

            // Assert
            Json.Array(result)
                .DeepEqual(output.AsJson())
                .ShouldBe(true);
            if (stdErr != null)
            {
                Json.Array(context.Log)
                    .DeepEqual(stdErr.AsJson())
                    .ShouldBe(true);
            }
        }

        public static IEnumerable<object[]> TestData
            => GetTestData().Select(item => (new object[] { item.Op, item.Input, item.Output, item.StdErr }));

        private static IEnumerable<TestItem> GetTestData()
            => Enumerable.Empty<TestItem>()
                .Concat(RangeTests())
                .Concat(LengthTests())
                .Concat(LimitTests())
                .Concat(KeysTests())
                .Concat(DebugTests())
                .Concat(SortTests())
                .Concat(ModuloTests())
                .Concat(HasInTests())
                .Concat(GetSetPathTests())
                .Concat(DelPathsTests())
                .Concat(TypeTests())
                .Concat(ToStringToNumberTests())
                .Concat(DateTests());

        private static IEnumerable<TestItem> RangeTests()
        {
            yield return new TestItem(
                new FunctionCall(Range.Builtin_1, new Literal(3)),
                "null",
                "[0, 1, 2]");
            yield return new TestItem(
                new FunctionCall(
                    Range.Builtin_1,
                    Concat.AllParams(new Literal(3), new Literal(4))),
                "null",
                "[0, 1, 2, 0, 1, 2, 3]");
            yield return new TestItem(
                new FunctionCall(Range.Builtin_2, new Literal(1), new Literal(3)),
                "null",
                "[1, 2]");
            yield return new TestItem(
                new FunctionCall(Range.Builtin_3, new Literal(1), new Literal(6), new Literal(2)),
                "null",
                "[1, 3, 5]");
        }

        private static IEnumerable<TestItem> LengthTests()
        {
            yield return new TestItem(
                new FunctionCall(Length.Builtin), "[1, 2, 3]", "[3]");
            yield return new TestItem(
                new FunctionCall(Length.Builtin), "null", "[1]");
            yield return new TestItem(
                new FunctionCall(Length.Builtin), "{\"a\": 1, \"b\": 2}", "[2]");
            yield return new TestItem(
                new FunctionCall(Length.Builtin), "\"abcdef\"", "[6]");
        }

        private static IEnumerable<TestItem> LimitTests()
        {
            yield return new TestItem(
                new FunctionCall(Limit.Builtin, new Literal(2), new Enumerate()),
                "[1, 2, 3]",
                "[1, 2]");
        }


        private static IEnumerable<TestItem> KeysTests()
        {
            yield return new TestItem(
                new FunctionCall(Keys.Builtin), "{\"a\": 1, \"b\": 2}", "[[\"a\", \"b\"]]");
            yield return new TestItem(
                new FunctionCall(Keys.Builtin), "[1, 2, 3, 4, 5]", "[[0, 1, 2, 3, 4]]");
        }

        private static IEnumerable<TestItem> DebugTests()
        {
            yield return new TestItem(
                new FunctionCall(Debug.Builtin),
                "[1, 2, 3]",
                "[[1, 2, 3]]",
                "[[\"DEBUG\", [1, 2, 3]]]");
        }

        private static IEnumerable<TestItem> SortTests()
        {
            yield return new TestItem(
                new FunctionCall(Sort.Builtin_0),
                "[3, [], \"4\", {}, true, false, null]",
                "[[null, false, true, 3, \"4\", [], {}]]");
            yield return new TestItem(
                new FunctionCall(Sort.Builtin_0), "[3, 4, 1, 2]", "[[1, 2, 3, 4]]");
            yield return new TestItem(
                new FunctionCall(Sort.Builtin_0),
                "[\"4\", \"12\"]",
                "[[\"12\", \"4\"]]");
            yield return new TestItem(
                new FunctionCall(Sort.Builtin_0),
                "[[3, 1, 2], [3, 0, 5], [3, 0]]",
                "[[[3, 0], [3, 0, 5], [3, 1, 2]]]");
            yield return new TestItem(
                new FunctionCall(Sort.Builtin_0),
                "[{\"a\": 2}, {\"a\": 0, \"b\": 0}, {\"a\": 1}]",
                "[[{\"a\": 1}, {\"a\": 2}, {\"a\": 0, \"b\": 0}]]");
        }

        private static IEnumerable<TestItem> ModuloTests()
        {
            yield return new TestItem(
                new FunctionCall(Modulo.Builtin, new Literal(7), new Literal(2)),
                "null",
                "[1]");
        }

        private static IEnumerable<TestItem> HasInTests()
        {
            yield return new TestItem(
                new FunctionCall(Has.Builtin, new Literal(2)),
                "[1, 2, 3]",
                "[true]");
        }

        private static IEnumerable<TestItem> GetSetPathTests()
        {
            yield return new TestItem(
                new FunctionCall(
                    GetPath.Builtin,
                    new Literal { Value = Json.ArrayParams(Json.Number(0), Json.Number(1)) }),
                "[[1, 2], 3]",
                "[2]");
            yield return new TestItem(
                new FunctionCall(
                    SetPath.Builtin,
                    new Literal { Value = Json.ArrayParams(Json.Number(0), Json.Number(1)) },
                    new Literal(100)),
                "[[1, 100], 3]",
                "[[[1, 100], 3]]");
        }

        private static IEnumerable<TestItem> DelPathsTests()
        {
            yield return new TestItem(
                new FunctionCall(
                    DelPaths.Builtin,
                    new Literal { Value = Json.ArrayParams(
                        Json.ArrayParams(Json.Number(2)),
                        Json.ArrayParams(Json.Number(1))) }),
                "[1, 2, 3]",
                "[[1]]");
        }

        private static IEnumerable<TestItem> TypeTests()
        {
            yield return new TestItem(new FunctionCall(JsonType.Builtin), "null", "[\"null\"]");
            yield return new TestItem(new FunctionCall(JsonType.Builtin), "true", "[\"boolean\"]");
            yield return new TestItem(new FunctionCall(JsonType.Builtin), "false", "[\"boolean\"]");
            yield return new TestItem(new FunctionCall(JsonType.Builtin), "100", "[\"number\"]");
            yield return new TestItem(new FunctionCall(JsonType.Builtin), "\"abc\"", "[\"string\"]");
            yield return new TestItem(new FunctionCall(JsonType.Builtin), "[]", "[\"array\"]");
            yield return new TestItem(new FunctionCall(JsonType.Builtin), "{}", "[\"object\"]");
        }

        private static IEnumerable<TestItem> ToStringToNumberTests()
        {
            yield return new TestItem(new FunctionCall(Tostring.Builtin), "null", "[\"null\"]");
            yield return new TestItem(new FunctionCall(Tonumber.Builtin), "\"100\"", "[100]");
        }

        private static IEnumerable<TestItem> DateTests()
        {
            yield return new TestItem(
                new FunctionCall(DateFunctions.FromDate),
                "\"2020-01-20T10:00:30Z\"",
                "[1579514430]");
            yield return new TestItem(
                new FunctionCall(DateFunctions.ToDate),
                "1579514430",
                "[\"2020-01-20T10:00:30.0000000+00:00\"]");
        }
    }
}
