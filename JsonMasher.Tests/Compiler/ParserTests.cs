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
using Ops = JsonMasher.Mashers.Operators;

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
                    .ComparingByValue<Builtin>()
                    .ComparingByMembers<PropertyDescriptor>()
                    .ComparingByMembers<Thunk>()
                    .ComparingByMembers<FunctionName>());
        }

        private static IEnumerable<TestItem> GetTestData()
            => Enumerable.Empty<TestItem>()
                .Concat(DotTests())
                .Concat(IfThenElseTests())
                .Concat(NotTests())
                .Concat(LiteralTests())
                .Concat(ArrayConstructionTests())
                .Concat(ObjectConstructionTests())
                .Concat(PlusMinusTests())
                .Concat(TimesDivisionTests())
                .Concat(ParenTests())
                .Concat(RelationalTests())
                .Concat(CommaTests())
                .Concat(AssignmentTests())
                .Concat(VariablesTest())
                .Concat(PipeTests())
                .Concat(EmptyTests());

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

        private static IEnumerable<TestItem> IfThenElseTests()
        {
            yield return new TestItem(
                "if true then 1 else 2 end",
                new IfThenElse {
                    Cond = new Literal(true),
                    Then = new Literal(1),
                    Else = new Literal(2)
                });
            yield return new TestItem(
                "if true then 1 elif true then 2 else 3 end",
                new IfThenElse {
                    Cond = new Literal(true),
                    Then = new Literal(1),
                    Else = new IfThenElse {
                        Cond = new Literal(true),
                        Then = new Literal(2),
                        Else = new Literal(3)
                    }
                });
        }

        private static IEnumerable<TestItem> NotTests()
        {
            yield return new TestItem("not .", new FunctionCall(Not.Builtin, Identity.Instance));
            yield return new TestItem(
                "true or false", 
                new FunctionCall(Or.Builtin, new Literal(true), new Literal(false)));
            yield return new TestItem(
                "true and false", 
                new FunctionCall(And.Builtin, new Literal(true), new Literal(false)));
            yield return new TestItem(
                "true or false and true", 
                new FunctionCall(
                    Or.Builtin, 
                    new Literal(true), 
                    new FunctionCall(
                        And.Builtin,
                        new Literal(false),
                        new Literal(true))));
            yield return new TestItem(
                "true or false or true", 
                new FunctionCall(
                    Or.Builtin, 
                    new FunctionCall(
                        Or.Builtin,
                        new Literal(true),
                        new Literal(false)), 
                    new Literal(true)));
            yield return new TestItem(
                "true and false and true", 
                new FunctionCall(
                    And.Builtin, 
                    new FunctionCall(
                        And.Builtin,
                        new Literal(true),
                        new Literal(false)), 
                    new Literal(true)));
            yield return new TestItem(
                "true and false or false and true", 
                new FunctionCall(
                    Or.Builtin, 
                    new FunctionCall(
                        And.Builtin,
                        new Literal(true),
                        new Literal(false)), 
                    new FunctionCall(
                        And.Builtin,
                        new Literal(false),
                        new Literal(true))));
        }

        private static IEnumerable<TestItem> ArrayConstructionTests()
        {
            yield return new TestItem("[1]", new ConstructArray {
                Elements = new Literal(1)
            });
            yield return new TestItem("[1, 2]", new ConstructArray {
                Elements = Concat.AllParams(
                    new Literal(1),
                    new Literal(2))
            });
            yield return new TestItem("[1, 2, \"a\"]", new ConstructArray {
                Elements = Concat.AllParams(
                    new Literal(1),
                    new Literal(2),
                    new Literal("a"))
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
                    new Literal(1))));
            yield return new TestItem("{\"a\":1}", new ConstructObject(
                new PropertyDescriptor(
                    "a", 
                    new Literal(1))));
            yield return new TestItem("{a:1, b:2}", new ConstructObject(
                new PropertyDescriptor(
                    "a", 
                    new Literal(1)),
                new PropertyDescriptor(
                    "b", 
                    new Literal(2))));
            yield return new TestItem("{\"a\":1, b:2}", new ConstructObject(
                new PropertyDescriptor(
                    "a", 
                    new Literal(1)),
                new PropertyDescriptor(
                    "b", 
                    new Literal(2))));
            yield return new TestItem("{\"a\":1, \"b\":2}", new ConstructObject(
                new PropertyDescriptor(
                    "a", 
                    new Literal(1)),
                new PropertyDescriptor(
                    "b", 
                    new Literal(2))));
            yield return new TestItem("{a:1, \"b\":2}", new ConstructObject(
                new PropertyDescriptor(
                    "a", 
                    new Literal(1)),
                new PropertyDescriptor(
                    "b", 
                    new Literal(2))));
            yield return new TestItem("{a:1, b: {c: 2}, d: 3}", new ConstructObject(
                new PropertyDescriptor(
                    "a", 
                    new Literal(1)),
                new PropertyDescriptor(
                    "b",
                    new ConstructObject(
                        new PropertyDescriptor(
                            "c", 
                            new Literal(2)))),
                new PropertyDescriptor(
                    "d", 
                    new Literal(3))));
        }

        private static IEnumerable<TestItem> LiteralTests()
        {
            yield return new TestItem("1", new Literal(1));
            yield return new TestItem("1.", new Literal(1));
            yield return new TestItem("12.3", new Literal(12.3));
            yield return new TestItem("\"a\"", new Literal("a"));
            yield return new TestItem("null", new Literal(Json.Null));
            yield return new TestItem("true", new Literal(Json.True));
            yield return new TestItem("false", new Literal(Json.False));
        }

        private static IEnumerable<TestItem> PlusMinusTests()
        {
            yield return new TestItem(
                "1 + 1", 
                new FunctionCall(
                    Plus.Builtin,
                    new Literal(1),
                    new Literal(1)));
            yield return new TestItem(
                "1 - 1", 
                new FunctionCall(
                    Minus.Builtin,
                    new Literal(1),
                    new Literal(1)));
            yield return new TestItem(
                "1 + 1 + 1", 
                new FunctionCall(
                    Plus.Builtin, 
                    new FunctionCall(
                        Plus.Builtin, 
                        new Literal(1),
                        new Literal(1)),
                    new Literal(1)));
            yield return new TestItem(
                "1 - 1 + 1", 
                new FunctionCall(
                    Plus.Builtin,
                    new FunctionCall(
                        Minus.Builtin,
                        new Literal(1),
                        new Literal(1)),
                    new Literal(1)));
            yield return new TestItem(
                "1 + 1 - 1", 
                new FunctionCall(
                    Minus.Builtin, 
                    new FunctionCall(
                        Plus.Builtin,
                        new Literal(1),
                        new Literal(1)),
                    new Literal(1)));
        }

        private static IEnumerable<TestItem> TimesDivisionTests()
        {
            yield return new TestItem(
                "1 + 2 * 2", 
                new FunctionCall(
                    Plus.Builtin,
                    new Literal(1),
                    new FunctionCall(
                        Times.Builtin,
                        new Literal(2),
                        new Literal(2))));
            yield return new TestItem(
                "1 + 2 / 2", 
                new FunctionCall(
                    Plus.Builtin,
                    new Literal(1),
                    new FunctionCall(
                        Divide.Builtin,
                        new Literal(2),
                        new Literal(2))));
        }

        private static IEnumerable<TestItem> ParenTests()
        {
            yield return new TestItem(
                "1 + (1 - 1)",
                new FunctionCall(
                    Plus.Builtin,
                    new Literal(1),
                    new FunctionCall(
                        Minus.Builtin,
                        new Literal(1),
                        new Literal(1))));
            yield return new TestItem("(. | .) | .", Compose.AllParams(
                Compose.AllParams(
                    Identity.Instance,
                    Identity.Instance),
                Identity.Instance
            ));
            yield return new TestItem(
                "(1 + 2) * 2",
                new FunctionCall(
                    Times.Builtin,
                    new FunctionCall(
                        Plus.Builtin,
                        new Literal(1),
                        new Literal(2)),
                    new Literal(2)));
        }

        private static IEnumerable<TestItem> RelationalTests()
        {
            yield return new TestItem(
                "1 == 2",
                new FunctionCall(EqualsEquals.Builtin, new Literal(1), new Literal(2)));
            yield return new TestItem(
                "1 < 2",
                new FunctionCall(Ops.LessThan.Builtin, new Literal(1), new Literal(2)));
            yield return new TestItem(
                "1 <= 2",
                new FunctionCall(Ops.LessThanOrEqual.Builtin, new Literal(1), new Literal(2)));
            yield return new TestItem(
                "1 > 2",
                new FunctionCall(Ops.GreaterThan.Builtin, new Literal(1), new Literal(2)));
            yield return new TestItem(
                "1 >= 2",
                new FunctionCall(Ops.GreaterThanOrEqual.Builtin, new Literal(1), new Literal(2)));
        }

        private static IEnumerable<TestItem> CommaTests()
        {
            yield return new TestItem("1, 2", Concat.AllParams(
                new Literal(1),
                new Literal(2)
            ));
            yield return new TestItem("1, 2, \"a\"", Concat.AllParams(
                new Literal(1),
                new Literal(2),
                new Literal("a")
            ));
            yield return new TestItem(
                "1, 2, \"a\" | . | 3, 4", 
                Compose.AllParams(
                    Concat.AllParams(
                        new Literal(1),
                        new Literal(2),
                        new Literal("a")),
                    Identity.Instance,
                    Concat.AllParams(
                        new Literal(3),
                        new Literal { Value = Json.Number(4) })));
        }

        private static IEnumerable<TestItem> AssignmentTests()
        {
            yield return new TestItem(". |= . + 2", new PipeAssignment {
                PathExpression = Identity.Instance,
                Masher = new FunctionCall(
                    Plus.Builtin,
                    Identity.Instance,
                    new Literal(2))
            });
            yield return new TestItem(". |= . + 2 | . |= . + 2", Compose.AllParams(
                new PipeAssignment {
                    PathExpression = Identity.Instance,
                    Masher = new FunctionCall(
                        Plus.Builtin,
                        Identity.Instance,
                        new Literal(2))
                },
                new PipeAssignment {
                    PathExpression = Identity.Instance,
                    Masher = new FunctionCall(
                        Plus.Builtin,
                        Identity.Instance,
                        new Literal(2))
                }));
        }

        private static IEnumerable<TestItem> VariablesTest()
        {
            yield return new TestItem(". + 2 as $test | .", new Let {
                Name = "test",
                Value = new FunctionCall(
                    Plus.Builtin,
                    Identity.Instance,
                    new Literal(2)),
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

        private static IEnumerable<TestItem> EmptyTests()
        {
            yield return new TestItem("empty", new FunctionCall(Empty.Builtin));
        }
    }
}
