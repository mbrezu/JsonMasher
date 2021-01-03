using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Mashers;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Primitives;
using Xunit;
using Ops = JsonMasher.Mashers.Builtins;

namespace JsonMasher.Tests.Builtins
{
    public class RelationalTests
    {
        private record TestItem(IJsonMasherOperator Op, string Input, string Output);

        [Theory]
        [MemberData(nameof(TestData))]
        public void RelationalTestsTheory(IJsonMasherOperator op, string input, string output)
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
                .Concat(EqualityTests())
                .Concat(LessThanTests())
                .Concat(LessThanOrEqualTests())
                .Concat(GreaterThanTests())
                .Concat(GreaterThanOrEqualTests());

        private static IEnumerable<TestItem> EqualityTests()
        {
            yield return new TestItem(
                new FunctionCall(EqualsEquals.Builtin, new Literal(2), new Literal(1)),
                "null",
                "[false]");
            yield return new TestItem(
                new FunctionCall(EqualsEquals.Builtin, new Literal(2), new Literal(2)),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(NotEquals.Builtin, new Literal(2), new Literal(2)),
                "null",
                "[false]");
            yield return new TestItem(
                new FunctionCall(NotEquals.Builtin, new Literal(1), new Literal(2)),
                "null",
                "[true]");
        }

        private static IEnumerable<TestItem> LessThanTests()
        {
            yield return new TestItem(
                new FunctionCall(Ops.LessThan.Builtin, new Literal(2), new Literal(2)),
                "null",
                "[false]");
            yield return new TestItem(
                new FunctionCall(Ops.LessThan.Builtin, new Literal(2), new Literal(3)),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(Ops.LessThan.Builtin, new Literal(3), new Literal(2)),
                "null",
                "[false]");
            yield return new TestItem(
                new FunctionCall(Ops.LessThan.Builtin, new Literal("a"), new Literal("a")),
                "null",
                "[false]");
            yield return new TestItem(
                new FunctionCall(Ops.LessThan.Builtin, new Literal("a"), new Literal("b")),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(Ops.LessThan.Builtin, new Literal("b"), new Literal("a")),
                "null",
                "[false]");
        }

        private static IEnumerable<TestItem> LessThanOrEqualTests()
        {
            yield return new TestItem(
                new FunctionCall(Ops.LessThanOrEqual.Builtin, new Literal(2), new Literal(2)),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(Ops.LessThanOrEqual.Builtin, new Literal(2), new Literal(3)),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(Ops.LessThanOrEqual.Builtin, new Literal(3), new Literal(2)),
                "null",
                "[false]");
            yield return new TestItem(
                new FunctionCall(Ops.LessThanOrEqual.Builtin, new Literal("a"), new Literal("a")),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(Ops.LessThanOrEqual.Builtin, new Literal("a"), new Literal("b")),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(Ops.LessThanOrEqual.Builtin, new Literal("b"), new Literal("a")),
                "null",
                "[false]");
        }

        private static IEnumerable<TestItem> GreaterThanTests()
        {
            yield return new TestItem(
                new FunctionCall(Ops.GreaterThan.Builtin, new Literal(2), new Literal(2)),
                "null",
                "[false]");
            yield return new TestItem(
                new FunctionCall(Ops.GreaterThan.Builtin, new Literal(3), new Literal(2)),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(Ops.GreaterThan.Builtin, new Literal(2), new Literal(3)),
                "null",
                "[false]");
            yield return new TestItem(
                new FunctionCall(Ops.GreaterThan.Builtin, new Literal("a"), new Literal("a")),
                "null",
                "[false]");
            yield return new TestItem(
                new FunctionCall(Ops.GreaterThan.Builtin, new Literal("b"), new Literal("a")),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(Ops.GreaterThan.Builtin, new Literal("a"), new Literal("b")),
                "null",
                "[false]");
        }

        private static IEnumerable<TestItem> GreaterThanOrEqualTests()
        {
            yield return new TestItem(
                new FunctionCall(Ops.GreaterThanOrEqual.Builtin, new Literal(2), new Literal(2)),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(Ops.GreaterThanOrEqual.Builtin, new Literal(2), new Literal(3)),
                "null",
                "[false]");
            yield return new TestItem(
                new FunctionCall(Ops.GreaterThanOrEqual.Builtin, new Literal(3), new Literal(2)),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(Ops.GreaterThanOrEqual.Builtin, new Literal("a"), new Literal("a")),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(Ops.GreaterThanOrEqual.Builtin, new Literal("a"), new Literal("b")),
                "null",
                "[false]");
            yield return new TestItem(
                new FunctionCall(Ops.GreaterThanOrEqual.Builtin, new Literal("b"), new Literal("a")),
                "null",
                "[true]");
        }
    }
}
