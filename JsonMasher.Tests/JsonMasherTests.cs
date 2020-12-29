using System.Linq;
using FluentAssertions;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Primitives;
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
            result.DeepEqual(data).Should().BeTrue();
        }

        [Fact]
        public void TestIdentity()
        {
            // Arrange
            var data = MakeArray();
            var op = Identity.Instance;

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
            var op = Compose.AllParams(Identity.Instance);

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
            var op = Enumerate.Instance;

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
            var op = Enumerate.Instance;

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual(MakeArray()).Should().BeTrue();
        }

        [Fact]
        public void ArrayLength()
        {
            // Arrange
            var data = MakeArray();
            var op = Length.Instance;

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual(Json.Number(3)).Should().BeTrue();
        }

        [Fact]
        public void ObjectLength()
        {
            // Arrange
            var data = MakeObject();
            var op = Length.Instance;

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual(Json.Number(3)).Should().BeTrue();
        }

        [Fact]
        public void ComposeArrayEnumerations()
        {
            // Arrange
            var data = MakeNestedArray();
            var op = Compose.AllParams(Enumerate.Instance, Enumerate.Instance);

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
            var op = Empty.Instance;

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
            var op = Compose.AllParams(Empty.Instance, Identity.Instance);

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
            var op = Compose.AllParams(Identity.Instance, Empty.Instance);

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
            var op = Concat.AllParams(Enumerate.Instance, Enumerate.Instance);

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
                Enumerate.Instance,
                Concat.AllParams(
                    Identity.Instance,
                    Identity.Instance));

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
            var op = new ConstructArray { Elements = Enumerate.Instance };

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
                Enumerate.Instance,
                new ConstructObject(
                    new PropertyDescriptor("a", Identity.Instance)));

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
                new PropertyDescriptor("a", Identity.Instance),
                new PropertyDescriptor("b", Enumerate.Instance));

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
                First = Enumerate.Instance,
                Second = new ConstructObject(
                    new PropertyDescriptor("a", Identity.Instance),
                    new PropertyDescriptor("b", Identity.Instance))};

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
        public void DebugTest()
        {
            // Arrange
            var data = MakeArray();
            var op = new Compose {
                First = Enumerate.Instance,
                Second = Debug.Instance
            };

            // Act
            var (result, context) = op.RunAsSequenceWithContext(data);
            result = result.ToList();

            // Assert
            Json.Array(result)
                .DeepEqual(data)
                .Should().BeTrue();
            context.Log.Count().Should().Be(3);
            for (int i = 0; i < 3; i++)
            {
                var obj = context.Log.ElementAt(i);
                obj.DeepEqual(Json.Number(i + 1)).Should().BeTrue();
            }
        }

        [Fact]
        public void LetAndGetVariableTest()
        {
            // Arrange
            var data = MakeArray();
            var op = new Let { 
                Value = Identity.Instance,
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
                    Value = Enumerate.Instance,
                    Name = "var",
                    Body = new GetVariable { Name = "var" }
                }
            }; 

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual(data).Should().BeTrue();
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
