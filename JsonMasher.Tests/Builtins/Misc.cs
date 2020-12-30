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
                .Concat(RangeTests());

        private static IEnumerable<TestItem> RangeTests()
        {
            yield return new TestItem(
                new FunctionCall(Range.Builtin, new Literal(3)),
                "null",
                "[0, 1, 2]");
            yield return new TestItem(
                new FunctionCall(
                    Range.Builtin, 
                    Concat.AllParams(new Literal(3), new Literal(4))),
                "null",
                "[0, 1, 2, 0, 1, 2, 3]");
        }
    }
}
