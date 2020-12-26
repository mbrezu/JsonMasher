using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Combinators;
using JsonMasher.Functions;
using JsonMasher.Primitives;
using Xunit;

namespace JsonMasher.Tests
{
    public class JsonMasherTests
    {
        // TODO: test conversion from JsonElement to Json
        [Fact]
        public void EmptySequence()
        {
            // Arrange
            Json data = MakeArray();
            var op = Compose.AllParams();

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.Should().BeEquivalentTo(data);
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
            result.Should().BeEquivalentTo(data);
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
            result.Should().BeEquivalentTo(data);
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
            result.Count().Should().Be(3);
            for (int i = 0; i < 3; i++)
            {
                var expectation = data.EnumerateArray().ElementAt(i);
                result.ElementAt(i).Should().BeEquivalentTo(expectation);
            }
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
            result.Count().Should().Be(3);
            var expectedValues = new List<double> { 1, 2, 3 };
            var actualValues = result.Select(x => x.GetNumber()).OrderBy(x => x).ToList();
            actualValues.Should().BeEquivalentTo(expectedValues);
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
            result.Should().BeEquivalentTo(Json.Number(3));
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
            result.Should().BeEquivalentTo(Json.Number(3));
        }

        [Fact]
        public void ComposeArrayEnumerations()
        {
            // Arrange
            var data = MakeNestedArray();
            var op = new Compose { First = Enumerate.Instance, Second = Enumerate.Instance };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Count().Should().Be(9);
            for (int i = 0; i < 0; i++)
            {
                var expectation = Json.Number(i + 1);
                result.ElementAt(i).Should().BeEquivalentTo(expectation);
            }
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
            var expectedValues = new List<double> { 1, 2, 3, 1, 2, 3 };
            result.Select(x => x.GetNumber()).Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void ConcatComponseEnumerations()
        {
            // Arrange
            var data = MakeArray();
            var op = new Compose {
                First = Enumerate.Instance,
                Second = new Concat {
                    First = Identity.Instance,
                    Second = Identity.Instance
                }
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            var expectedValues = new List<double> { 1, 1, 2, 2, 3, 3 };
            result.Select(x => x.GetNumber()).Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void LiteralTest()
        {
            // Arrange
            var data = Json.Null;
            var op = new Literal { Value = Json.Number(1) };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            var expectedValues = new List<double> { 1 };
            result.Select(x => x.GetNumber()).Should().BeEquivalentTo(expectedValues);
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
            var expectedValues = new List<double> { 1 };
            result.Select(x => x.GetNumber()).Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void SelectorTestNumber()
        {
            // Arrange
            var data = MakeArray();
            var op = new Selector { Index = new Literal { Value = Json.Number(1) } };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            var expectedValues = new List<double> { 2 };
            result.Select(x => x.GetNumber()).Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void SelectorTestSequence()
        {
            // Arrange
            var data = MakeArray();
            var op = new Selector { Index = Concat.AllParams(
                new Literal { Value = Json.Number(1) },
                new Literal { Value = Json.Number(2) },
                new Literal { Value = Json.Number(0) }
            ) };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            var expectedValues = new List<double> { 2, 3, 1 };
            result.Select(x => x.GetNumber()).Should().BeEquivalentTo(expectedValues);
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
            var expectedValues = new List<double> { 3 };
            result.Select(x => x.GetNumber()).Should().BeEquivalentTo(expectedValues);
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
            var expectedValues = new List<double> { 2 };
            result.Count().Should().Be(1);
            result.First().Should().BeEquivalentTo(data);
        }

        [Fact]
        public void ConstructObjectTest()
        {
            // Arrange
            var data = MakeArray();
            var op = new Compose {
                First = Enumerate.Instance,
                Second = new ConstructObject(
                    new ConstructObject.PropertyDescriptor("a", Identity.Instance))};

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Count().Should().Be(3);
            for (int i = 0; i < 3; i++)
            {
                var obj = result.ElementAt(i);
                obj.Type.Should().Be(JsonValueType.Object);
                obj.GetElementAt("a").GetNumber().Should().Be(i + 1);
            }
        }

        [Fact]
        public void ConstructObjectInnerEnumerate()
        {
            // Arrange
            var data = MakeArray();
            var op = new ConstructObject(
                new ConstructObject.PropertyDescriptor("a", Identity.Instance),
                new ConstructObject.PropertyDescriptor("b", Enumerate.Instance));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Count().Should().Be(3);
            result.Count().Should().Be(3);
            for (int i = 0; i < 3; i++)
            {
                var obj = result.ElementAt(i);
                obj.Type.Should().Be(JsonValueType.Object);
                obj.GetElementAt("b").GetNumber().Should().Be(i + 1);
                obj.GetElementAt("a").Type.Should().Be(JsonValueType.Array);
                obj.GetElementAt("a").Should().BeEquivalentTo(data);
            }
        }

        [Fact]
        public void ConstructObjectTestTwoKeys()
        {
            // Arrange
            var data = MakeArray();
            var op = new Compose {
                First = Enumerate.Instance,
                Second = new ConstructObject(
                    new ConstructObject.PropertyDescriptor("a", Identity.Instance),
                    new ConstructObject.PropertyDescriptor("b", Identity.Instance))};

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Count().Should().Be(3);
            for (int i = 0; i < 3; i++)
            {
                var obj = result.ElementAt(i);
                obj.Type.Should().Be(JsonValueType.Object);
                obj.GetElementAt("a").GetNumber().Should().Be(i + 1);
                obj.GetElementAt("b").GetNumber().Should().Be(i + 1);
            }
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
            result.ToList();

            // Assert
            context.Log.Count().Should().Be(3);
            for (int i = 0; i < 3; i++)
            {
                var obj = context.Log.ElementAt(i);
                obj.Should().BeEquivalentTo(Json.Number(i + 1));
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
            result.Should().BeEquivalentTo(data);
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
            result.Should().BeEquivalentTo(data);
        }

        // TODO: switch tests to using DeepEqual
        // TODO: lexer
        // TODO: parser
        // TODO: pretty printing (FancyPen?) for Json values
        // TODO: pretty printing operators
        // TODO: n-ary operators
        // TODO: relational operators: <, <=, ==, >, >=
        // TODO: boolean operators: and, or, not
        // TODO: if/elif
        // TODO: has(key), keys, map, map_values
        // TODO: del, to_entries, from_entries, select, error, transpose, range
        // TODO: assignments
        // TODO: min/max/group/sort
        // TODO: .. (recurse)
        // TODO: variable bindings (set in context?)
        // TODO: error handling
        // TODO: stack trace
        // TODO: test coverage
        // TODO: documentation
        // TODO: nuget packages

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
