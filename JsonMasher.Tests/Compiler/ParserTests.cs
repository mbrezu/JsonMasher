using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Compiler;
using JsonMasher.Mashers;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Operators;
using JsonMasher.Mashers.Primitives;
using Xunit;

namespace JsonMasher.Tests.Compiler
{
    public class ParserTests
    {
        private record TestItem(string program, IJsonMasherOperator expectation);

        public static IEnumerable<object[]> TestData
            => GetTestData().Select(item => new object[] { item.program, item.expectation });

        [Theory]
        [MemberData(nameof(TestData))]
        public void ParseTest(string program, IJsonMasherOperator expectation)
        {
            // Arrange
            var parser = new Parser();

            // Act
            var result = parser.Parse(program);

            // Assert
            result.Should().BeEquivalentTo(
                expectation,
                options => options.RespectingRuntimeTypes());
        }

        private static IEnumerable<TestItem> GetTestData()
            => Enumerable.Empty<TestItem>()
                .Concat(DotTests())
                .Concat(LiteralTests())
                .Concat(PlusMinusTests())
                .Concat(TimesTests())
                .Concat(ParenTests())
                .Concat(RelationalTests())
                .Concat(PipeTests());

        private static IEnumerable<TestItem> DotTests()
        {
            yield return new TestItem("", Identity.Instance);
            yield return new TestItem(".", Identity.Instance);
            yield return new TestItem(".a", new StringSelector { Key = "a" });
            yield return new TestItem(".a.b", Compose.AllParams(
                new StringSelector { Key = "a" },
                new StringSelector { Key = "b" }));
            yield return new TestItem(".a[]", Compose.AllParams(
                new StringSelector { Key = "a" },
                Enumerate.Instance));
            yield return new TestItem(".[]", Enumerate.Instance);
            yield return new TestItem(".[].a", Compose.AllParams(
                Enumerate.Instance,
                new StringSelector { Key = "a" }));
            yield return new TestItem(".[.].a", Compose.AllParams(
                new Selector { Index = Identity.Instance },
                new StringSelector { Key = "a" }));
        }

        private static IEnumerable<TestItem> LiteralTests()
        {
            yield return new TestItem("1", new Literal { Value = Json.Number(1) });
            yield return new TestItem("1.", new Literal { Value = Json.Number(1) });
            yield return new TestItem("12.3", new Literal { Value = Json.Number(12.3) });
            yield return new TestItem("\"a\"", new Literal { Value = Json.String("a") });
        }

        private static IEnumerable<TestItem> PlusMinusTests()
        {
            yield return new TestItem("1 + 1", new BinaryOperator {
                First = new Literal { Value = Json.Number(1) },
                Second = new Literal { Value = Json.Number(1) },
                Operator = Plus.Operator
            });
            yield return new TestItem("1 - 1", new BinaryOperator {
                First = new Literal { Value = Json.Number(1) },
                Second = new Literal { Value = Json.Number(1) },
                Operator = Minus.Operator
            });
            yield return new TestItem("1 + 1 + 1", new BinaryOperator {
                First = new BinaryOperator {
                    First = new Literal { Value = Json.Number(1) },
                    Second = new Literal { Value = Json.Number(1) },
                    Operator = Plus.Operator
                },
                Second = new Literal { Value = Json.Number(1) },
                Operator = Plus.Operator
            });
            yield return new TestItem("1 - 1 + 1", new BinaryOperator {
                First = new BinaryOperator {
                    First = new Literal { Value = Json.Number(1) },
                    Second = new Literal { Value = Json.Number(1) },
                    Operator = Minus.Operator
                },
                Second = new Literal { Value = Json.Number(1) },
                Operator = Plus.Operator
            });
            yield return new TestItem("1 + 1 - 1", new BinaryOperator {
                First = new BinaryOperator {
                    First = new Literal { Value = Json.Number(1) },
                    Second = new Literal { Value = Json.Number(1) },
                    Operator = Plus.Operator
                },
                Second = new Literal { Value = Json.Number(1) },
                Operator = Minus.Operator
            });
        }

        private static IEnumerable<TestItem> TimesTests()
        {
            yield return new TestItem("1 + 2 * 2", new BinaryOperator {
                First = new Literal { Value = Json.Number(1) },
                Second = new BinaryOperator {
                    First = new Literal { Value = Json.Number(2) },
                    Second = new Literal { Value = Json.Number(2) },
                    Operator = Times.Operator
                },
                Operator = Plus.Operator
            });
        }

        private static IEnumerable<TestItem> ParenTests()
        {
            yield return new TestItem("1 + (1 - 1)", new BinaryOperator {
                First = new Literal { Value = Json.Number(1) },
                Second = new BinaryOperator {
                    First = new Literal { Value = Json.Number(1) },
                    Second = new Literal { Value = Json.Number(1) },
                    Operator = Minus.Operator
                },
                Operator = Plus.Operator
            });
            yield return new TestItem("(. | .) | .", Compose.AllParams(
                Compose.AllParams(
                    Identity.Instance,
                    Identity.Instance),
                Identity.Instance
            ));
            yield return new TestItem("(1 + 2) * 2", new BinaryOperator {
                First = new BinaryOperator {
                    First = new Literal { Value = Json.Number(1) },
                    Second = new Literal { Value = Json.Number(2) },
                    Operator = Plus.Operator
                },
                Second = new Literal { Value = Json.Number(2) },
                Operator = Times.Operator
            });
        }

        private static IEnumerable<TestItem> RelationalTests()
        {
            yield return new TestItem("1 == 2", new BinaryOperator {
                First = new Literal { Value = Json.Number(1) },
                Second = new Literal { Value = Json.Number(2) },
                Operator = EqualsEquals.Operator
            });
        }

        private static IEnumerable<TestItem> PipeTests()
        {
            yield return new TestItem(". | .", Compose.AllParams(
                Identity.Instance,
                Identity.Instance));

            yield return new TestItem(". | . | .", Compose.AllParams(
                Identity.Instance,
                Identity.Instance,
                Identity.Instance));

            yield return new TestItem(".a | .[].c | .d", Compose.AllParams(
                new StringSelector { Key = "a" },
                Compose.AllParams(
                    Enumerate.Instance,
                    new StringSelector { Key = "c"}
                ),
                new StringSelector { Key = "d" }));
        }
    }
}
