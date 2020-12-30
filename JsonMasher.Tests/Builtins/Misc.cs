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
        private record TestItem(IJsonMasherOperator Op, string Input, string Output);

        [Theory]
        [MemberData(nameof(TestData))]
        public void BooleanTestsTheory(IJsonMasherOperator op, string input, string output)
        {
            // Arrange

            // Act
            var result = op.RunAsSequence(input.AsJson());

            // Assert
            Json.Array(result)
                .DeepEqual(output.AsJson())
                .Should().BeTrue();
        }

        public static IEnumerable<object[]> TestData
            => GetTestData().Select(item => (new object[] { item.Op, item.Input, item.Output }));

        private static IEnumerable<TestItem> GetTestData()
            => Enumerable.Empty<TestItem>()
                .Concat(RangeTests())
                .Concat(LengthTests());

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
    }
}
