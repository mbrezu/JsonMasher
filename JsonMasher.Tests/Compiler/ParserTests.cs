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
                options => options
                    .RespectingRuntimeTypes()
                    .WithStrictOrdering()
                    .ComparingByMembers<PropertyDescriptor>());
        }

        private static IEnumerable<TestItem> GetTestData()
            => Enumerable.Empty<TestItem>()
                .Concat(DotTests())
                .Concat(LiteralTests())
                .Concat(ArrayConstructionTests())
                .Concat(ObjectConstructionTests())
                .Concat(PlusMinusTests())
                .Concat(TimesTests())
                .Concat(ParenTests())
                .Concat(RelationalTests())
                .Concat(CommaTests())
                .Concat(AssignmentTests())
                .Concat(VariablesTest())
                .Concat(PipeTests());

        private static IEnumerable<TestItem> DotTests()
        {
            yield return new TestItem("", Identity.Instance);
            yield return new TestItem(".", Identity.Instance);
            yield return new TestItem(".a", new StringSelector { Key = "a" });
            yield return new TestItem(".\"a\"", new StringSelector { Key = "a" });
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
            yield return new TestItem(".[.].a[][]", Compose.AllParams(
                new Selector { Index = Identity.Instance },
                new StringSelector { Key = "a" },
                Enumerate.Instance,
                Enumerate.Instance));
            yield return new TestItem(".[][]", Compose.AllParams(
                Enumerate.Instance,
                Enumerate.Instance));
            yield return new TestItem(".[][][]", Compose.AllParams(
                Enumerate.Instance,
                Enumerate.Instance,
                Enumerate.Instance));
            yield return new TestItem(".[][][][]", Compose.AllParams(
                Enumerate.Instance,
                Enumerate.Instance,
                Enumerate.Instance,
                Enumerate.Instance));
        }

        private static IEnumerable<TestItem> ArrayConstructionTests()
        {
            yield return new TestItem("[1]", new ConstructArray {
                Elements = new Literal { Value = Json.Number(1) }
            });
            yield return new TestItem("[1, 2]", new ConstructArray {
                Elements = Concat.AllParams(
                    new Literal { Value = Json.Number(1) },
                    new Literal { Value = Json.Number(2) })
            });
            yield return new TestItem("[1, 2, \"a\"]", new ConstructArray {
                Elements = Concat.AllParams(
                    new Literal { Value = Json.Number(1) },
                    new Literal { Value = Json.Number(2) },
                    new Literal { Value = Json.String("a") })
            });
        }

        private static IEnumerable<TestItem> ObjectConstructionTests()
        {
            yield return new TestItem("{a:.}", new ConstructObject(
                new PropertyDescriptor(
                    "a", 
                    Identity.Instance)));
            yield return new TestItem("{a:1}", new ConstructObject(
                new PropertyDescriptor(
                    "a", 
                    new Literal { Value = Json.Number(1) })));
            yield return new TestItem("{\"a\":1}", new ConstructObject(
                new PropertyDescriptor(
                    "a", 
                    new Literal { Value = Json.Number(1) })));
            yield return new TestItem("{a:1, b:2}", new ConstructObject(
                new PropertyDescriptor(
                    "a", 
                    new Literal { Value = Json.Number(1) }),
                new PropertyDescriptor(
                    "b", 
                    new Literal { Value = Json.Number(2) })));
            yield return new TestItem("{\"a\":1, b:2}", new ConstructObject(
                new PropertyDescriptor(
                    "a", 
                    new Literal { Value = Json.Number(1) }),
                new PropertyDescriptor(
                    "b", 
                    new Literal { Value = Json.Number(2) })));
            yield return new TestItem("{\"a\":1, \"b\":2}", new ConstructObject(
                new PropertyDescriptor(
                    "a", 
                    new Literal { Value = Json.Number(1) }),
                new PropertyDescriptor(
                    "b", 
                    new Literal { Value = Json.Number(2) })));
            yield return new TestItem("{a:1, \"b\":2}", new ConstructObject(
                new PropertyDescriptor(
                    "a", 
                    new Literal { Value = Json.Number(1) }),
                new PropertyDescriptor(
                    "b", 
                    new Literal { Value = Json.Number(2) })));
            yield return new TestItem("{a:1, b: {c: 2}, d: 3}", new ConstructObject(
                new PropertyDescriptor(
                    "a", 
                    new Literal { Value = Json.Number(1) }),
                new PropertyDescriptor(
                    "b",
                    new ConstructObject(
                        new PropertyDescriptor(
                            "c", 
                            new Literal { Value = Json.Number(2) }))),
                new PropertyDescriptor(
                    "d", 
                    new Literal { Value = Json.Number(3) })));
        }

        private static IEnumerable<TestItem> LiteralTests()
        {
            yield return new TestItem("1", new Literal { Value = Json.Number(1) });
            yield return new TestItem("1.", new Literal { Value = Json.Number(1) });
            yield return new TestItem("12.3", new Literal { Value = Json.Number(12.3) });
            yield return new TestItem("\"a\"", new Literal { Value = Json.String("a") });
            yield return new TestItem("null", new Literal { Value = Json.Null });
            yield return new TestItem("true", new Literal { Value = Json.True });
            yield return new TestItem("false", new Literal { Value = Json.False });
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

        private static IEnumerable<TestItem> CommaTests()
        {
            yield return new TestItem("1, 2", Concat.AllParams(
                new Literal { Value = Json.Number(1) },
                new Literal { Value = Json.Number(2) }
            ));
            yield return new TestItem("1, 2, \"a\"", Concat.AllParams(
                new Literal { Value = Json.Number(1) },
                new Literal { Value = Json.Number(2) },
                new Literal { Value = Json.String("a") }
            ));
            yield return new TestItem(
                "1, 2, \"a\" | . | 3, 4", 
                Compose.AllParams(
                    Concat.AllParams(
                        new Literal { Value = Json.Number(1) },
                        new Literal { Value = Json.Number(2) },
                        new Literal { Value = Json.String("a") }),
                    Identity.Instance,
                    Concat.AllParams(
                        new Literal { Value = Json.Number(3) },
                        new Literal { Value = Json.Number(4) })));
        }

        private static IEnumerable<TestItem> AssignmentTests()
        {
            yield return new TestItem(". |= . + 2", new PipeAssignment {
                PathExpression = Identity.Instance,
                Masher = new BinaryOperator {
                    First = Identity.Instance,
                    Second = new Literal { Value = Json.Number(2) },
                    Operator = Plus.Operator
                }
            });
            yield return new TestItem(". |= . + 2 | . |= . + 2", Compose.AllParams(
                new PipeAssignment {
                    PathExpression = Identity.Instance,
                    Masher = new BinaryOperator {
                        First = Identity.Instance,
                        Second = new Literal { Value = Json.Number(2) },
                        Operator = Plus.Operator
                    }
                },
                new PipeAssignment {
                    PathExpression = Identity.Instance,
                    Masher = new BinaryOperator {
                        First = Identity.Instance,
                        Second = new Literal { Value = Json.Number(2) },
                        Operator = Plus.Operator
                    }
                }));
        }

        private static IEnumerable<TestItem> VariablesTest()
        {
            yield return new TestItem(". + 2 as $test | .", new Let {
                Name = "test",
                Value = new BinaryOperator {
                    First = Identity.Instance,
                    Second = new Literal { Value = Json.Number(2) },
                    Operator = Plus.Operator
                },
                Body = Identity.Instance
            });
            yield return new TestItem("$test", new GetVariable { Name = "test" });
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

            yield return new TestItem(". | . | . | .", Compose.AllParams(
                Identity.Instance,
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
