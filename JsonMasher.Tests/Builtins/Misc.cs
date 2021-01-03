using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
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
                .Should().BeTrue();
            if (stdErr != null)
            {
                Json.Array(context.Log)
                    .DeepEqual(stdErr.AsJson())
                    .Should().BeTrue();
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
                .Concat(DebugTests());

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
    }
}
