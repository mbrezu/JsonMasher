using System;
using System.Linq;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Primitives;
using Shouldly;
using Xunit;

namespace JsonMasher.Tests
{
    public class JsonMasherTests
    {
        [Fact]
        public void EmptySequence()
        {
            // Arrange
            Json data = MakeArray();
            var op = Compose.AllParams();

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual(data).ShouldBe(true);
        }

        [Fact]
        public void TestIdentity()
        {
            // Arrange
            var data = MakeArray();
            var op = new Identity();

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual(data).ShouldBe(true);
        }

        [Fact]
        public void IdentityInSequence()
        {
            // Arrange
            var data = MakeArray();
            var op = Compose.AllParams(new Identity());

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual(data).ShouldBe(true);
        }

        [Fact]
        public void EnumerateArray()
        {
            // Arrange
            var data = MakeArray();
            var op = new Enumerate();

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual(data).ShouldBe(true);
        }

        [Fact]
        public void EnumerateObject()
        {
            // Arrange
            var data = MakeObject();
            var op = new Enumerate();

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual(MakeArray()).ShouldBe(true);
        }

        [Fact]
        public void ComposeArrayEnumerations()
        {
            // Arrange
            var data = MakeNestedArray();
            var op = Compose.AllParams(new Enumerate(), new Enumerate());

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(1, 2, 3, 4, 5, 6, 7, 8, 9))
                .ShouldBe(true);
        }

        [Fact]
        public void TestEmpty()
        {
            // Arrange
            var data = MakeNestedArray();
            var op = new FunctionCall(Empty.Builtin);

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Count().ShouldBe(0);
        }

        [Fact]
        public void TestEmptyFirstInComposition()
        {
            // Arrange
            var data = MakeNestedArray();
            var op = Compose.AllParams(new FunctionCall(Empty.Builtin), new Identity());

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Count().ShouldBe(0);
        }

        [Fact]
        public void TestEmptySecondInComposition()
        {
            // Arrange
            var data = MakeNestedArray();
            var op = Compose.AllParams(new Identity(), new FunctionCall(Empty.Builtin));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Count().ShouldBe(0);
        }

        [Fact]
        public void ConcatEnumerations()
        {
            // Arrange
            var data = MakeArray();
            var op = Concat.AllParams(new Enumerate(), new Enumerate());

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual(Utils.JsonNumberArray(1, 2, 3, 1, 2, 3)).ShouldBe(true);
        }

        [Fact]
        public void ConcatComponseEnumerations()
        {
            // Arrange
            var data = MakeArray();
            var op = Compose.AllParams(
                new Enumerate(),
                Concat.AllParams(new Identity(), new Identity())
            );

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual(Utils.JsonNumberArray(1, 1, 2, 2, 3, 3)).ShouldBe(true);
        }

        [Fact]
        public void LiteralTest()
        {
            // Arrange
            var data = Json.Null;
            var op = new Literal(1);

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual(Utils.JsonNumberArray(1)).ShouldBe(true);
        }

        [Fact]
        public void StringSelectorTest()
        {
            // Arrange
            var data = MakeObject();
            var op = new StringSelector { Key = "a" };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual(Utils.JsonNumberArray(1)).ShouldBe(true);
        }

        [Fact]
        public void OptionalStringSelectorTest()
        {
            // Arrange
            var data = MakeArray();
            var op = new StringSelector { Key = "a", IsOptional = true };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void SelectorTestNumber()
        {
            // Arrange
            var data = MakeArray();
            var op = new Selector { Index = new Literal(1) };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual(Utils.JsonNumberArray(2)).ShouldBe(true);
        }

        [Fact]
        public void OptionalSelector()
        {
            // Arrange
            var data = MakeObject();
            var op = new Selector { Index = new Literal(1), IsOptional = true };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void SelectorTestNegativeNumber()
        {
            // Arrange
            var data = MakeArray();
            var op = new Selector { Index = new Literal(-2) };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual(Utils.JsonNumberArray(2)).ShouldBe(true);
        }

        [Fact]
        public void SelectorTestSequence()
        {
            // Arrange
            var data = MakeArray();
            var op = new Selector
            {
                Index = Concat.AllParams(
                    new Literal(1),
                    new Literal { Value = Json.Number(2) },
                    new Literal { Value = Json.Number(0) }
                ),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual(Utils.JsonNumberArray(2, 3, 1)).ShouldBe(true);
        }

        [Fact]
        public void SelectorTestString()
        {
            // Arrange
            var data = MakeObject();
            var op = new Selector { Index = new Literal { Value = Json.String("c") } };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual(Utils.JsonNumberArray(3)).ShouldBe(true);
        }

        [Fact]
        public void ConstructArrayTest()
        {
            // Arrange
            var data = MakeArray();
            var op = new ConstructArray { Elements = new Enumerate() };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Count().ShouldBe(1);
            result.First().DeepEqual(data).ShouldBe(true);
        }

        [Fact]
        public void ConstructObjectTest()
        {
            // Arrange
            var data = MakeArray();
            var op = Compose.AllParams(
                new Enumerate(),
                new ConstructObject(new PropertyDescriptor(new Literal("a"), new Identity()))
            );

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(
                    Json.ArrayParams(
                        Json.ObjectParams(new JsonProperty("a", Json.Number(1))),
                        Json.ObjectParams(new JsonProperty("a", Json.Number(2))),
                        Json.ObjectParams(new JsonProperty("a", Json.Number(3)))
                    )
                )
                .ShouldBe(true);
        }

        [Fact]
        public void ConstructObjectInnerEnumerate()
        {
            // Arrange
            var data = MakeArray();
            var op = new ConstructObject(
                new PropertyDescriptor(new Literal("a"), new Identity()),
                new PropertyDescriptor(new Literal("b"), new Enumerate())
            );

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(
                    Json.ArrayParams(
                        Json.ObjectParams(
                            new JsonProperty("a", data),
                            new JsonProperty("b", Json.Number(1))
                        ),
                        Json.ObjectParams(
                            new JsonProperty("a", data),
                            new JsonProperty("b", Json.Number(2))
                        ),
                        Json.ObjectParams(
                            new JsonProperty("a", data),
                            new JsonProperty("b", Json.Number(3))
                        )
                    )
                )
                .ShouldBe(true);
        }

        [Fact]
        public void ConstructObjectTestTwoKeys()
        {
            // Arrange
            var data = MakeArray();
            var op = new Compose
            {
                First = new Enumerate(),
                Second = new ConstructObject(
                    new PropertyDescriptor(new Literal("a"), new Identity()),
                    new PropertyDescriptor(new Literal("b"), new Identity())
                ),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(
                    Json.ArrayParams(
                        Json.ObjectParams(
                            new JsonProperty("a", Json.Number(1)),
                            new JsonProperty("b", Json.Number(1))
                        ),
                        Json.ObjectParams(
                            new JsonProperty("a", Json.Number(2)),
                            new JsonProperty("b", Json.Number(2))
                        ),
                        Json.ObjectParams(
                            new JsonProperty("a", Json.Number(3)),
                            new JsonProperty("b", Json.Number(3))
                        )
                    )
                )
                .ShouldBe(true);
        }

        [Fact]
        public void IfThenElseTrue()
        {
            // Arrange
            var data = MakeArray();
            var op = new IfThenElse
            {
                Cond = new Literal(true),
                Then = new Literal(1),
                Else = new Literal(2),
            };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("1".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void IfThenElseFalse()
        {
            // Arrange
            var data = MakeArray();
            var op = new IfThenElse
            {
                Cond = new Literal(false),
                Then = new Literal(1),
                Else = new Literal(2),
            };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("2".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void AlternativeFails()
        {
            // Arrange
            var data = MakeArray();
            var op = new Alternative { First = new Literal(false), Second = new Literal(1) };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("1".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void AlternativeSucceeds()
        {
            // Arrange
            var data = MakeArray();
            var op = new Alternative { First = new Literal(2), Second = new Literal(1) };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("2".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void TryCatch()
        {
            // Arrange
            var data = Json.Number(3);
            var op = new TryCatch
            {
                TryBody = new Enumerate(),
                CatchBody = new FunctionCall(Times.Builtin, new Identity(), new Literal(2)),
            };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.GetString().ShouldBe("Can't enumerate Number.Can't enumerate Number.");
        }

        [Fact]
        public void TryCatchWithoutTry()
        {
            // Arrange
            var data = Json.Number(3);
            var op = new TryCatch { TryBody = new Enumerate() };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Count().ShouldBe(0);
        }

        [Fact]
        public void Reduce()
        {
            // Arrange
            var data = "[1, 2, 3]".AsJson();
            var op = new ReduceForeach
            {
                Name = "item",
                Inputs = new Enumerate(),
                Initial = new Literal(0),
                Update = new FunctionCall(
                    Plus.Builtin,
                    new Identity(),
                    new GetVariable { Name = "item" }
                ),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Count().ShouldBe(1);
            result.First().DeepEqual(Json.Number(6)).ShouldBe(true);
        }

        [Fact]
        public void IsInfiniteTest()
        {
            // Arrange
            var data = Json.ArrayParams(
                Json.Number(double.NegativeInfinity),
                Json.Number(double.PositiveInfinity),
                Json.Number(0)
            );
            var op = Compose.AllParams(new Enumerate(), new FunctionCall(IsInfinite.Builtin));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[true, true, false]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void IsNormalTest()
        {
            // Arrange
            var data = Json.ArrayParams(Json.Number(0));
            var op = Compose.AllParams(new Enumerate(), new FunctionCall(IsNormal.Builtin));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[false]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void ErrorTest()
        {
            // Arrange
            var data = Json.ArrayParams(Json.Number(0));
            var op = Compose.AllParams(new Literal("not good"), new FunctionCall(Error.Builtin));

            // Act
            Action action = () => op.RunAsSequence(data).ToList();

            // Assert
            action.ShouldThrow<JsonMasherException>(ex => ex.Message == "not good");
        }

        [Fact]
        public void FirstTest()
        {
            // Arrange
            var data = Json.ArrayParams(Json.Number(0), Json.Number(1));
            var op = new FunctionCall(First.Builtin, new Enumerate());

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[0]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void GroupByTest()
        {
            // Arrange
            var data = Json.ArrayParams(
                Json.Number(0),
                Json.Number(1),
                Json.Number(0),
                Json.Number(2)
            );
            var op = new FunctionCall(
                GroupBy.Builtin,
                new Literal { Value = "[[0], [1], [0], [2]]".AsJson() }
            );

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[[[0, 0], [1], [2]]]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void MinByTest()
        {
            // Arrange
            var data = Json.ArrayParams(
                Json.Number(0),
                Json.Number(1),
                Json.Number(0),
                Json.Number(2)
            );
            var op = new FunctionCall(
                MinBy.Builtin,
                new Literal { Value = "[[0], [1], [0], [2]]".AsJson() }
            );

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[0]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void MinByTestEmpty()
        {
            // Arrange
            var data = Json.ArrayParams();
            var op = new FunctionCall(MinBy.Builtin, new Literal { Value = "[]".AsJson() });

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[null]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void MaxByTest()
        {
            // Arrange
            var data = Json.ArrayParams(
                Json.Number(0),
                Json.Number(1),
                Json.Number(0),
                Json.Number(2)
            );
            var op = new FunctionCall(
                MaxBy.Builtin,
                new Literal { Value = "[[0], [1], [0], [2]]".AsJson() }
            );

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[2]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void MaxByTestEmpty()
        {
            // Arrange
            var data = Json.ArrayParams();
            var op = new FunctionCall(MaxBy.Builtin, new Literal { Value = "[]".AsJson() });

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[null]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void StrIndicesTest()
        {
            // Arrange
            var data = Json.String("abacabacab");
            var op = new FunctionCall(StrIndices.Builtin, new Literal("acab"));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[[2, 6]]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void ContainsStringsTest()
        {
            // Arrange
            var data = Json.String("abacabacab");
            var op = new FunctionCall(Contains.Builtin, new Literal("acab"));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[true]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void ContainsArraysTest()
        {
            // Arrange
            var data = "[1,2,3]".AsJson();
            var op = new FunctionCall(Contains.Builtin, new Literal { Value = "[1,3]".AsJson() });

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[true]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void ContainsDictionaryTest()
        {
            // Arrange
            var data = "{\"a\":1,\"b\":2,\"c\":3}".AsJson();
            var op = new FunctionCall(
                Contains.Builtin,
                new Literal { Value = "{\"a\":1,\"c\":3}".AsJson() }
            );

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[true]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void ImplodeTest()
        {
            // Arrange
            var data = "[97, 98, 99, 100]".AsJson();
            var op = new FunctionCall(Implode.Builtin);

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[\"abcd\"]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void ToJsonTest()
        {
            // Arrange
            var data = "[1, 2, 3, {}]".AsJson();
            var op = new FunctionCall(ToJson.Builtin);

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[\"[1, 2, 3, {}]\"]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void NowTest()
        {
            // Arrange
            var data = Json.Null;
            var op = new FunctionCall(DateFunctions.Now);

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Count().ShouldBe(1);
            result.First().Type.ShouldBe(JsonValueType.Number);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var utcNow = DateTime.UtcNow;
            var assertion =
                Math.Abs(result.First().GetNumber() - (utcNow - epoch).TotalSeconds) < 1;
            assertion.ShouldBe(true);
        }

        [Fact]
        public void LabelBreakTest()
        {
            // Arrange
            var data = Json.Null;
            var op = new Label
            {
                Name = "out",
                Body = Concat.AllParams(
                    new Literal(1),
                    new Literal(2),
                    new Literal(3),
                    new Break { Label = "out" },
                    new Literal(4)
                ),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[1,2,3]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void LabelBreakPseudoNestedTest()
        {
            // Arrange
            var data = Json.Null;
            var op = new Label
            {
                Name = "out",
                Body = Concat.AllParams(
                    new Literal(1),
                    new Literal(2),
                    new Literal(3),
                    new Label
                    {
                        Name = "inner",
                        Body = new Break { Label = "inner" },
                    },
                    new Literal(4)
                ),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[1,2,3,4]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void IsNanTest()
        {
            // Arrange
            var data = Json.ArrayParams(Json.Number(double.NaN), Json.Number(0));
            var op = Compose.AllParams(new Enumerate(), new FunctionCall(IsNan.Builtin));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[true, false]".AsJson()).ShouldBe(true);
        }

        private static Json MakeArray() => "[1,2,3]".AsJson();

        private static Json MakeObject()
        {
            return Json.ObjectParams(
                new JsonProperty("a", Json.Number(1)),
                new JsonProperty("b", Json.Number(2)),
                new JsonProperty("c", Json.Number(3))
            );
        }

        private static Json MakeNestedArray() => "[[1,2,3],[4,5,6],[7,8,9]]".AsJson();
    }
}
