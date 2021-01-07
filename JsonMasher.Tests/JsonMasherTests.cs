using System.Linq;
using FluentAssertions;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Primitives;
using Xunit;
using JsonMasher.JsonRepresentation;

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
            result.DeepEqual(data).Should().BeTrue();
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
            result.DeepEqual(data).Should().BeTrue();
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
            result.DeepEqual(data).Should().BeTrue();
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
            Json.Array(result).DeepEqual(data).Should().BeTrue();
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
            Json.Array(result).DeepEqual(MakeArray()).Should().BeTrue();
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
                .Should().BeTrue();
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
            result.Count().Should().Be(0);
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
            result.Count().Should().Be(0);
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
            result.Count().Should().Be(0);
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
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(1, 2, 3, 1, 2, 3))
                .Should().BeTrue();
        }

        [Fact]
        public void ConcatComponseEnumerations()
        {
            // Arrange
            var data = MakeArray();
            var op = Compose.AllParams(
                new Enumerate(),
                Concat.AllParams(
                    new Identity(),
                    new Identity()));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(1, 1, 2, 2, 3, 3))
                .Should().BeTrue();
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
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(1))
                .Should().BeTrue();
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
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(1))
                .Should().BeTrue();
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
            Json.Array(result)
                .DeepEqual("[]".AsJson())
                .Should().BeTrue();
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
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(2))
                .Should().BeTrue();
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
            Json.Array(result)
                .DeepEqual("[]".AsJson())
                .Should().BeTrue();
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
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(2))
                .Should().BeTrue();
        }

        [Fact]
        public void SelectorTestSequence()
        {
            // Arrange
            var data = MakeArray();
            var op = new Selector { Index = Concat.AllParams(
                new Literal(1),
                new Literal { Value = Json.Number(2) },
                new Literal { Value = Json.Number(0) }
            ) };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(2, 3, 1))
                .Should().BeTrue();
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
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(3))
                .Should().BeTrue();
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
            result.Count().Should().Be(1);
            result.First().DeepEqual(data).Should().BeTrue();
        }

        [Fact]
        public void ConstructObjectTest()
        {
            // Arrange
            var data = MakeArray();
            var op = Compose.AllParams(
                new Enumerate(),
                new ConstructObject(
                    new PropertyDescriptor(new Literal("a"), new Identity())));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Json.ArrayParams(
                    Json.ObjectParams(new JsonProperty("a", Json.Number(1))),
                    Json.ObjectParams(new JsonProperty("a", Json.Number(2))),
                    Json.ObjectParams(new JsonProperty("a", Json.Number(3)))))
                .Should().BeTrue();
        }

        [Fact]
        public void ConstructObjectInnerEnumerate()
        {
            // Arrange
            var data = MakeArray();
            var op = new ConstructObject(
                new PropertyDescriptor(new Literal("a"), new Identity()),
                new PropertyDescriptor(new Literal("b"), new Enumerate()));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Json.ArrayParams(
                    Json.ObjectParams(
                        new JsonProperty("a", data),
                        new JsonProperty("b", Json.Number(1))),
                    Json.ObjectParams(
                        new JsonProperty("a", data),
                        new JsonProperty("b", Json.Number(2))),
                    Json.ObjectParams(
                        new JsonProperty("a", data),
                        new JsonProperty("b", Json.Number(3)))))
                .Should().BeTrue();
        }

        [Fact]
        public void ConstructObjectTestTwoKeys()
        {
            // Arrange
            var data = MakeArray();
            var op = new Compose {
                First = new Enumerate(),
                Second = new ConstructObject(
                    new PropertyDescriptor(new Literal("a"), new Identity()),
                    new PropertyDescriptor(new Literal("b"), new Identity()))};

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Json.ArrayParams(
                    Json.ObjectParams(
                        new JsonProperty("a", Json.Number(1)),
                        new JsonProperty("b", Json.Number(1))),
                    Json.ObjectParams(
                        new JsonProperty("a", Json.Number(2)),
                        new JsonProperty("b", Json.Number(2))),
                    Json.ObjectParams(
                        new JsonProperty("a", Json.Number(3)),
                        new JsonProperty("b", Json.Number(3)))))
                .Should().BeTrue();
        }

        [Fact]
        public void LetAndGetVariableTest()
        {
            // Arrange
            var data = MakeArray();
            var op = new Let { 
                Value = new Identity(),
                Name = "var",
                Body = new GetVariable { Name = "var" }
            };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual(data).Should().BeTrue();
        }

        [Fact]
        public void LetAndGetVariableSequenceTest()
        {
            // Arrange
            var data = MakeArray();
            var op = new ConstructArray{
                Elements = new Let { 
                    Value = new Enumerate(),
                    Name = "var",
                    Body = new GetVariable { Name = "var" }
                }
            }; 

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual(data).Should().BeTrue();
        }

        [Fact]
        public void IfThenElseTrue()
        {
            // Arrange
            var data = MakeArray();
            var op = new IfThenElse {
                Cond = new Literal(true),
                Then = new Literal(1),
                Else = new Literal(2)
            }; 

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("1".AsJson()).Should().BeTrue();
        }

        [Fact]
        public void IfThenElseFalse()
        {
            // Arrange
            var data = MakeArray();
            var op = new IfThenElse {
                Cond = new Literal(false),
                Then = new Literal(1),
                Else = new Literal(2)
            }; 

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("2".AsJson()).Should().BeTrue();
        }

        [Fact]
        public void AlternativeFails()
        {
            // Arrange
            var data = MakeArray();
            var op = new Alternative {
                First = new Literal(false),
                Second = new Literal(1),
            }; 

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("1".AsJson()).Should().BeTrue();
        }

        [Fact]
        public void AlternativeSucceeds()
        {
            // Arrange
            var data = MakeArray();
            var op = new Alternative {
                First = new Literal(2),
                Second = new Literal(1),
            }; 

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("2".AsJson()).Should().BeTrue();
        }

        [Fact]
        public void TryCatch()
        {
            // Arrange
            var data = Json.Number(3);
            var op = new TryCatch {
                TryBody = new Enumerate(),
                CatchBody = new FunctionCall(
                    Times.Builtin,
                    new Identity(),
                    new Literal(2))
            };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.GetString().Should().Be("Can't enumerate Number.Can't enumerate Number.");
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
            result.Count().Should().Be(0);
        }

        [Fact]
        public void Reduce()
        {
            // Arrange
            var data = "[1, 2, 3]".AsJson();
            var op = new ReduceForeach {
                Name = "item",
                Inputs = new Enumerate(),
                Initial = new Literal(0),
                Update = new FunctionCall(
                    Plus.Builtin, new Identity(), new GetVariable { Name = "item"} )
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Count().Should().Be(1);
            result.First().DeepEqual(Json.Number(6)).Should().BeTrue();
        }

        [Fact]
        public void IsInfiniteTest()
        {
            // Arrange
            var data = Json.ArrayParams(
                Json.Number(double.NegativeInfinity),
                Json.Number(double.PositiveInfinity),
                Json.Number(0));
            var op = Compose.AllParams(
                new Enumerate(),
                new FunctionCall(IsInfinite.Builtin)
            );

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[true, true, false]".AsJson()).Should().BeTrue();
        }

        [Fact]
        public void IsNormalTest()
        {
            // Arrange
            var data = Json.ArrayParams(Json.Number(0));
            var op = Compose.AllParams(
                new Enumerate(),
                new FunctionCall(IsNormal.Builtin)
            );

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[false]".AsJson()).Should().BeTrue();
        }

        private static Json MakeArray()
        {
            return Json.ArrayParams(
                Json.Number(1),
                Json.Number(2),
                Json.Number(3)
            );
        }

        private static Json MakeObject()
        {
            return Json.ObjectParams(
                new JsonProperty("a", Json.Number(1)),
                new JsonProperty("b", Json.Number(2)),
                new JsonProperty("c", Json.Number(3))
            );
        }

        private static Json MakeNestedArray()
        {
            return Json.ArrayParams(
                Json.ArrayParams(
                    Json.Number(1),
                    Json.Number(2),
                    Json.Number(3)
                ),
                Json.ArrayParams(
                    Json.Number(4),
                    Json.Number(5),
                    Json.Number(6)
                ),
                Json.ArrayParams(
                    Json.Number(7),
                    Json.Number(8),
                    Json.Number(9)
                )
            );
        }
    }
}
