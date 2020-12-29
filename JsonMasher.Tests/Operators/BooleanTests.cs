using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Mashers;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Operators;
using JsonMasher.Mashers.Primitives;
using Xunit;
using Ops = JsonMasher.Mashers.Operators;

namespace JsonMasher.Tests.Operators
{
    public class BooleanTests
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
                .Concat(AndTests())
                .Concat(OrTests())
                .Concat(NotTests());

        private static IEnumerable<TestItem> AndTests()
        {
            yield return new TestItem(
                new FunctionCall(And.Builtin, new Literal(true), new Literal(true)),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(And.Builtin, new Literal(true), new Literal(false)),
                "null",
                "[false]");
            yield return new TestItem(
                new FunctionCall(And.Builtin, new Literal(false), new Literal(true)),
                "null",
                "[false]");
            yield return new TestItem(
                new FunctionCall(And.Builtin, new Literal(false), new Literal(false)),
                "null",
                "[false]");
        }
        private static IEnumerable<TestItem> OrTests()
        {
            yield return new TestItem(
                new FunctionCall(Or.Builtin, new Literal(true), new Literal(true)),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(Or.Builtin, new Literal(true), new Literal(false)),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(Or.Builtin, new Literal(false), new Literal(true)),
                "null",
                "[true]");
            yield return new TestItem(
                new FunctionCall(Or.Builtin, new Literal(false), new Literal(false)),
                "null",
                "[false]");
        }

        private static IEnumerable<TestItem> NotTests()
        {
            yield return new TestItem(
                new FunctionCall(Not.Builtin, new Literal(true)),
                "null",
                "[false]");
            yield return new TestItem(
                new FunctionCall(Not.Builtin, new Literal(false)),
                "null",
                "[true]");
        }
    }
}
