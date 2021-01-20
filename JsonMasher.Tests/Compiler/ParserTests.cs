using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Compiler;
using JsonMasher.Mashers;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Primitives;
using Xunit;
using Ops = JsonMasher.Mashers.Builtins;
using JsonMasher.JsonRepresentation;
using System;
using JsonMasher.Mashers.Combinators.LetMatchers;

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
            var (result, _) = parser.Parse(program);

            // Assert
            result.Should().BeEquivalentTo(
                expectation,
                options => options
                    .RespectingRuntimeTypes()
                    .WithStrictOrdering()
                    .ComparingByValue<Builtin>()
                    .ComparingByMembers<PropertyDescriptor>()
                    .ComparingByMembers<FunctionName>()
                    .ComparingByValue<Enumerate>()
                    .ComparingByValue<Identity>());
        }

        private static IEnumerable<TestItem> GetTestData()
            => Enumerable.Empty<TestItem>()
                .Concat(DotTests())
                .Concat(IfThenElseTests())
                .Concat(BooleanTests())
                .Concat(LiteralTests())
                .Concat(ArrayConstructionTests())
                .Concat(ObjectConstructionTests())
                .Concat(PlusMinusTests())
                .Concat(TimesDivisionModuloTests())
                .Concat(ParenTests())
                .Concat(RelationalTests())
                .Concat(CommaTests())
                .Concat(AssignmentTests())
                .Concat(VariablesTest())
                .Concat(PipeTests())
                .Concat(EmptyTests())
                .Concat(FunctionTests())
                .Concat(AlternativeTests())
                .Concat(TryCatchTests())
                .Concat(ReduceForeachTests())
                .Concat(LabelBreakTests());

        private static IEnumerable<TestItem> DotTests()
        {
            yield return new TestItem("", new Identity());
            yield return new TestItem(".", new Identity());
            yield return new TestItem(".a", new StringSelector { Key = "a" });
            yield return new TestItem(".a?", new StringSelector { Key = "a", IsOptional = true });
            yield return new TestItem(".a.b?", Compose.AllParams(
                new StringSelector { Key = "a" },
                new StringSelector { Key = "b", IsOptional = true }));
            yield return new TestItem(".\"a\"?", new StringSelector { Key = "a", IsOptional = true });
            yield return new TestItem(".\"a\"", new StringSelector { Key = "a" });
            yield return new TestItem(".a.b", Compose.AllParams(
                new StringSelector { Key = "a" },
                new StringSelector { Key = "b" }));
            yield return new TestItem(".a[]", Compose.AllParams(
                new StringSelector { Key = "a" },
                new Enumerate()));
            yield return new TestItem(".[]", new Enumerate());
            yield return new TestItem(".[].a", Compose.AllParams(
                new Enumerate(),
                new StringSelector { Key = "a" }));
            yield return new TestItem(".[.].a", Compose.AllParams(
                new Selector { Index = new Identity() },
                new StringSelector { Key = "a" }));
            yield return new TestItem(".[.]?", Compose.AllParams(
                new Selector { Index = new Identity(), IsOptional = true }));
            yield return new TestItem(".[1:2]", Compose.AllParams(
                new SliceSelector { From = new Literal(1), To = new Literal(2) }));
            yield return new TestItem(".[1:2]?", Compose.AllParams(
                new SliceSelector { From = new Literal(1), To = new Literal(2), IsOptional = true }));
            yield return new TestItem(".[1:]", Compose.AllParams(
                new SliceSelector { From = new Literal(1) }));
            yield return new TestItem(".[1:]?", Compose.AllParams(
                new SliceSelector { From = new Literal(1), IsOptional = true }));
            yield return new TestItem(".[:2]", Compose.AllParams(
                new SliceSelector { To = new Literal(2) }));
            yield return new TestItem(".[:2]?", Compose.AllParams(
                new SliceSelector { To = new Literal(2), IsOptional = true }));
            yield return new TestItem(".[.].a[][]", Compose.AllParams(
                new Selector { Index = new Identity() },
                new StringSelector { Key = "a" },
                new Enumerate(),
                new Enumerate()));
            yield return new TestItem(".[][]", Compose.AllParams(
                new Enumerate(),
                new Enumerate()));
            yield return new TestItem(".[][][]", Compose.AllParams(
                new Enumerate(),
                new Enumerate(),
                new Enumerate()));
            yield return new TestItem(".[][][][]", Compose.AllParams(
                new Enumerate(),
                new Enumerate(),
                new Enumerate(),
                new Enumerate()));

            yield return new TestItem("..", new FunctionCall(new FunctionName("recurse", 0)));

            yield return new TestItem("$obj[.]", new Selector { 
                Target = new GetVariable { Name = "obj" }, 
                Index = new Identity()
            });
            yield return new TestItem("$obj[1][2]", new Selector {
                Target = new Selector { 
                    Target = new GetVariable { Name = "obj" }, 
                    Index = new Literal(1)
                },
                Index = new Literal(2)
            });
            yield return new TestItem("$obj[10:20]", new SliceSelector { 
                Target = new GetVariable { Name = "obj" }, 
                From = new Literal(10),
                To = new Literal(20)
            });
            yield return new TestItem("$obj[]", new Enumerate { 
                Target = new GetVariable { Name = "obj" }
            });
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

        private static IEnumerable<TestItem> BooleanTests()
        {
            yield return new TestItem(
                ". | not", 
                Compose.AllParams(new Identity(), new FunctionCall(new FunctionName("not", 0))));
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
            yield return new TestItem("[]", new ConstructArray {
            });
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
            yield return new TestItem("{}", new ConstructObject());
            yield return new TestItem("{a:.}", new ConstructObject(
                new PropertyDescriptor(
                    new Literal("a"), 
                    new Identity())));
            yield return new TestItem("{a:1}", new ConstructObject(
                new PropertyDescriptor(
                    new Literal("a"), 
                    new Literal(1))));
            yield return new TestItem("{\"a\":1}", new ConstructObject(
                new PropertyDescriptor(
                    new Literal("a"), 
                    new Literal(1))));
            yield return new TestItem("{(\"a\" + \"b\"):1}", new ConstructObject(
                new PropertyDescriptor(
                    new FunctionCall(Plus.Builtin, new Literal("a"), new Literal("b")),
                    new Literal(1))));
            yield return new TestItem("{\"a\":1}", new ConstructObject(
                new PropertyDescriptor(
                    new Literal("a"), 
                    new Literal(1))));
            yield return new TestItem("{a:1, b:2}", new ConstructObject(
                new PropertyDescriptor(
                    new Literal("a"), 
                    new Literal(1)),
                new PropertyDescriptor(
                    new Literal("b"), 
                    new Literal(2))));
            yield return new TestItem("{\"a\":1, b:2}", new ConstructObject(
                new PropertyDescriptor(
                    new Literal("a"), 
                    new Literal(1)),
                new PropertyDescriptor(
                    new Literal("b"), 
                    new Literal(2))));
            yield return new TestItem("{\"a\":1, \"b\":2}", new ConstructObject(
                new PropertyDescriptor(
                    new Literal("a"), 
                    new Literal(1)),
                new PropertyDescriptor(
                    new Literal("b"), 
                    new Literal(2))));
            yield return new TestItem("{a:1, \"b\":2}", new ConstructObject(
                new PropertyDescriptor(
                    new Literal("a"), 
                    new Literal(1)),
                new PropertyDescriptor(
                    new Literal("b"), 
                    new Literal(2))));
            yield return new TestItem("{a:1, b: {c: 2}, d: 3}", new ConstructObject(
                new PropertyDescriptor(
                    new Literal("a"), 
                    new Literal(1)),
                new PropertyDescriptor(
                    new Literal("b"),
                    new ConstructObject(
                        new PropertyDescriptor(
                            new Literal("c"), 
                            new Literal(2)))),
                new PropertyDescriptor(
                    new Literal("d"),
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
                    Minus.Builtin_2,
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
                        Minus.Builtin_2,
                        new Literal(1),
                        new Literal(1)),
                    new Literal(1)));
            yield return new TestItem(
                "1 + 1 - 1", 
                new FunctionCall(
                    Minus.Builtin_2, 
                    new FunctionCall(
                        Plus.Builtin,
                        new Literal(1),
                        new Literal(1)),
                    new Literal(1)));
            yield return new TestItem(
                "- 1", 
                new FunctionCall(Minus.Builtin_1, new Literal(1)));
            yield return new TestItem(
                "- .", 
                new FunctionCall(Minus.Builtin_1, new Identity()));
        }

        private static IEnumerable<TestItem> TimesDivisionModuloTests()
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
            yield return new TestItem(
                "1 + 7 % 2", 
                new FunctionCall(
                    Plus.Builtin,
                    new Literal(1),
                    new FunctionCall(
                        Modulo.Builtin,
                        new Literal(7),
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
                        Minus.Builtin_2,
                        new Literal(1),
                        new Literal(1))));
            yield return new TestItem("(. | .) | .", Compose.AllParams(
                Compose.AllParams(
                    new Identity(),
                    new Identity()),
                new Identity()
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
                "1 != 2",
                new FunctionCall(NotEquals.Builtin, new Literal(1), new Literal(2)));
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
                    new Identity(),
                    Concat.AllParams(
                        new Literal(3),
                        new Literal { Value = Json.Number(4) })));
        }

        private static IEnumerable<TestItem> AssignmentTests()
        {
            yield return new TestItem(". |= . + 2", new Assignment {
                PathExpression = new Identity(),
                Masher = new FunctionCall(
                    Plus.Builtin,
                    new Identity(),
                    new Literal(2))
            });
            yield return new TestItem(". |= . + 2 | . |= . + 2", Compose.AllParams(
                new Assignment {
                    PathExpression = new Identity(),
                    Masher = new FunctionCall(
                        Plus.Builtin,
                        new Identity(),
                        new Literal(2))
                },
                new Assignment {
                    PathExpression = new Identity(),
                    Masher = new FunctionCall(
                        Plus.Builtin,
                        new Identity(),
                        new Literal(2))
                }));
            yield return new TestItem(". = . + 2", new Assignment {
                PathExpression = new Identity(),
                Masher = new FunctionCall(
                    Plus.Builtin,
                    new Identity(),
                    new Literal(2)),
                UseWholeValue = true
            });
            yield return new TestItem(". += 2", new Assignment {
                PathExpression = new Identity(),
                Masher = new FunctionCall(
                    Plus.Builtin,
                    new Identity(),
                    new Literal(2)),
            });
            yield return new TestItem(". -= 2", new Assignment {
                PathExpression = new Identity(),
                Masher = new FunctionCall(
                    Minus.Builtin_2,
                    new Identity(),
                    new Literal(2)),
            });
            yield return new TestItem(". *= 2", new Assignment {
                PathExpression = new Identity(),
                Masher = new FunctionCall(
                    Times.Builtin,
                    new Identity(),
                    new Literal(2)),
            });
            yield return new TestItem(". /= 2", new Assignment {
                PathExpression = new Identity(),
                Masher = new FunctionCall(
                    Divide.Builtin,
                    new Identity(),
                    new Literal(2)),
            });
            yield return new TestItem(". %= 2", new Assignment {
                PathExpression = new Identity(),
                Masher = new FunctionCall(
                    Modulo.Builtin,
                    new Identity(),
                    new Literal(2)),
            });
            yield return new TestItem(". //= 2", new Assignment {
                PathExpression = new Identity(),
                Masher = new Alternative {
                    First = new Identity(),
                    Second = new Literal(2)
                }
            });
        }

        private static IEnumerable<TestItem> VariablesTest()
        {
            yield return new TestItem(". + 2 as $test | .", new Let {
                Matcher = new ValueMatcher("test"),
                Value = new FunctionCall(
                    Plus.Builtin,
                    new Identity(),
                    new Literal(2)),
                Body = new Identity()
            });
            yield return new TestItem("$test", new GetVariable { Name = "test" });
        }

        private static IEnumerable<TestItem> PipeTests()
        {
            yield return new TestItem(". | .", Compose.AllParams(
                new Identity(),
                new Identity()));

            yield return new TestItem(". | . | .", Compose.AllParams(
                new Identity(),
                new Identity(),
                new Identity()));

            yield return new TestItem(". | . | . | .", Compose.AllParams(
                new Identity(),
                new Identity(),
                new Identity(),
                new Identity()));

            yield return new TestItem(".a | .[].c | .d", Compose.AllParams(
                new StringSelector { Key = "a" },
                Compose.AllParams(
                    new Enumerate(),
                    new StringSelector { Key = "c"}
                ),
                new StringSelector { Key = "d" }));
        }

        private static IEnumerable<TestItem> EmptyTests()
        {
            yield return new TestItem("empty", new FunctionCall(new FunctionName("empty", 0)));
        }

        private static IEnumerable<TestItem> FunctionTests()
        {
            yield return new TestItem(
                "def test: 1;",
                new FunctionDefinition {
                    Name = "test",
                    Body = new Literal(1)
                });

            yield return new TestItem(
                "def test: 1; test",
                Compose.AllParams(
                    new FunctionDefinition {
                        Name = "test",
                        Body = new Literal(1)
                    },
                    new FunctionCall(new FunctionName("test", 0))));

            yield return new TestItem(
                "def test(a): 1 + a;",
                new FunctionDefinition {
                    Name = "test",
                    Arguments = new List<string> { "a" },
                    Body = new FunctionCall(
                        Plus.Builtin,
                        new Literal(1),
                        new FunctionCall(new FunctionName("a", 0)))
                });

            yield return new TestItem(
                "def test($a): 1 + $a;",
                new FunctionDefinition {
                    Name = "test",
                    Arguments = new List<string> { "a" },
                    Body = new Let {
                        Matcher = new ValueMatcher("a"),
                        Value = new FunctionCall(new FunctionName("a" ,0)),
                        Body = new FunctionCall(
                            Plus.Builtin,
                            new Literal(1),
                            new GetVariable { Name = "a" })
                    }
                });

            yield return new TestItem(
                "def point(x; y): [x, y];",
                new FunctionDefinition {
                    Name = "point",
                    Arguments = new List<string> { "x", "y" },
                    Body = new ConstructArray {
                        Elements = Concat.AllParams(
                            new FunctionCall(new FunctionName("x", 0)),
                            new FunctionCall(new FunctionName("y", 0)))
                    }
                });

            yield return new TestItem(
                "def point($x; $y): [$x, $y];",
                new FunctionDefinition {
                    Name = "point",
                    Arguments = new List<string> { "x", "y" },
                    Body = new Let {
                        Matcher = new ValueMatcher("x"),
                        Value = new FunctionCall(new FunctionName("x", 0)),
                        Body = new Let {
                            Matcher = new ValueMatcher("y"),
                            Value = new FunctionCall(new FunctionName("y", 0)),
                            Body = new ConstructArray {
                                Elements = Concat.AllParams(
                                    new GetVariable { Name = "x" },
                                    new GetVariable { Name = "y" })
                            }
                        }
                    }
                });

            yield return new TestItem(
                "def point(x; y; z): [x, y, z];",
                new FunctionDefinition {
                    Name = "point",
                    Arguments = new List<string> { "x", "y", "z" },
                    Body = new ConstructArray {
                        Elements = Concat.AllParams(
                            new FunctionCall(new FunctionName("x", 0)),
                            new FunctionCall(new FunctionName("y", 0)),
                            new FunctionCall(new FunctionName("z", 0)))
                    }
                });

            yield return new TestItem(
                "def point(x; y): [x, y]; point(1; 2)",
                Compose.AllParams(
                    new FunctionDefinition {
                        Name = "point",
                        Arguments = new List<string> { "x", "y" },
                        Body = new ConstructArray {
                            Elements = Concat.AllParams(
                                new FunctionCall(new FunctionName("x", 0)),
                                new FunctionCall(new FunctionName("y", 0)))
                        }
                    },
                    new FunctionCall(
                        new FunctionName("point", 2),
                        new Literal(1),
                        new Literal(2))));
        }

        private static IEnumerable<TestItem> AlternativeTests()
        {
            yield return new TestItem("1 // 2", new Alternative {
                First = new Literal(1),
                Second = new Literal(2)
            });
            yield return new TestItem("1 // 2 // 3", new Alternative {
                First = new Literal(1),
                Second = new Alternative {
                    First = new Literal(2),
                    Second = new Literal(3)
                }
            });
        }

        private static IEnumerable<TestItem> TryCatchTests()
        {
            yield return new TestItem("(1, 2, 3)?", new TryCatch {
                TryBody = Concat.AllParams(
                    new Literal(1),
                    new Literal(2),
                    new Literal(3)
                )
            });
            yield return new TestItem("-1?", new FunctionCall(
                Minus.Builtin_1,
                new TryCatch {
                    TryBody = new Literal(1)
                }));
            yield return new TestItem("try 1", new TryCatch {
                TryBody = new Literal(1)
            });
            yield return new TestItem("try 1 catch 2", new TryCatch {
                TryBody = new Literal(1),
                CatchBody = new Literal(2)
            });
        }

        private static IEnumerable<TestItem> ReduceForeachTests()
        {
            yield return new TestItem("reduce .[] as $item (0; . + $item)", new ReduceForeach {
                Name = "item",
                Inputs = new Enumerate(),
                Initial = new Literal(0),
                Update = new FunctionCall(
                    Plus.Builtin, new Identity(), new GetVariable { Name = "item" })
            });
            yield return new TestItem("foreach .[] as $item (0; . + $item)", new ReduceForeach {
                Name = "item",
                Inputs = new Enumerate(),
                Initial = new Literal(0),
                Update = new FunctionCall(
                    Plus.Builtin, new Identity(), new GetVariable { Name = "item" }),
                IsForeach = true
            });
            yield return new TestItem("foreach .[] as $item (0; . + $item; . * .)", new ReduceForeach {
                Name = "item",
                Inputs = new Enumerate(),
                Initial = new Literal(0),
                Update = new FunctionCall(
                    Plus.Builtin, new Identity(), new GetVariable { Name = "item" }),
                IsForeach = true,
                Extract = new FunctionCall(Times.Builtin, new Identity(), new Identity())
            });
        }

        private static IEnumerable<TestItem> LabelBreakTests()
        {
            yield return new TestItem("label $out | break $out", new Label {
                Name = "out",
                Body = new Break { Label = "out" },
            });
            yield return new TestItem("label $out | 1, 2, break $out, 3", new Label {
                Name = "out",
                Body = Concat.AllParams(
                    new Literal(1),
                    new Literal(2),
                    new Break { Label = "out" },
                    new Literal(3))
            });
        }
    }
}
