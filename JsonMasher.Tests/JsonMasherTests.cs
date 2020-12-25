using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Combinators;
using JsonMasher.Primitives;
using Xunit;

namespace JsonMasher.Tests
{
    public class JsonMasher
    {
        // TODO: test conversion from JsonElement to Json
        [Fact]
        public void EmptySequence()
        {
            // Arrange
            JsonArray data = MakeArray();
            var masher = Compose.AllParams();

            // Act
            var result = RunAsScalar(data, masher);

            // Assert
            result.Should().BeEquivalentTo(data);
        }

        [Fact]
        public void TestIdentity()
        {
            // Arrange
            var data = MakeArray();
            var masher = Identity.Instance;

            // Act
            var result = RunAsScalar(data, masher);

            // Assert
            result.Should().BeEquivalentTo(data);
        }

        [Fact]
        public void IdentityInSequence()
        {
            // Arrange
            var data = MakeArray();
            var masher = Compose.AllParams(Identity.Instance);

            // Act
            var result = RunAsScalar(data, masher);

            // Assert
            result.Should().BeEquivalentTo(data);
        }

        [Fact]
        public void EnumerateArray()
        {
            // Arrange
            var data = MakeArray();
            var masher = Enumerate.Instance;

            // Act
            var result = RunAsSequence(data, masher);

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
            var masher = Enumerate.Instance;

            // Act
            var result = RunAsSequence(data, masher);

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
            var masher = Length.Instance;

            // Act
            var result = RunAsScalar(data, masher);

            // Assert
            result.Should().BeEquivalentTo(Json.Number(3));
        }

        [Fact]
        public void ObjectLength()
        {
            // Arrange
            var data = MakeObject();
            var masher = Length.Instance;

            // Act
            var result = RunAsScalar(data, masher);

            // Assert
            result.Should().BeEquivalentTo(Json.Number(3));
        }

        [Fact]
        public void ComposeArrayEnumerations()
        {
            // Arrange
            var data = MakeNestedArray();
            var masher = new Compose { First = Enumerate.Instance, Second = Enumerate.Instance };

            // Act
            var result = RunAsSequence(data, masher);

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
            var masher = Empty.Instance;

            // Act
            var result = RunAsSequence(data, masher);

            // Assert
            result.Count().Should().Be(0);
        }

        [Fact]
        public void TestEmptyFirstInComposition()
        {
            // Arrange
            var data = MakeNestedArray();
            var masher = new Compose { First = Empty.Instance, Second = Identity.Instance };

            // Act
            var result = RunAsSequence(data, masher);

            // Assert
            result.Count().Should().Be(0);
        }

        [Fact]
        public void TestEmptySecondInComposition()
        {
            // Arrange
            var data = MakeNestedArray();
            var masher = new Compose { First = Identity.Instance, Second = Empty.Instance };

            // Act
            var result = RunAsSequence(data, masher);

            // Assert
            result.Count().Should().Be(0);
        }

        private static Json RunAsScalar(Json data, IJsonMasher masher)
            => masher.Mash(data.AsEnumerable()).First();

        private static IEnumerable<Json> RunAsSequence(Json data, IJsonMasher masher)
            => masher.Mash(data.AsEnumerable());

        private static JsonArray MakeArray()
        {
            return new JsonArray(new[] {
                Json.Number(1),
                Json.Number(2),
                Json.Number(3)
            });
        }

        private static Json MakeObject()
        {
            return new JsonObject(new [] {
                new JsonProperty("a", Json.Number(1)),
                new JsonProperty("b", Json.Number(2)),
                new JsonProperty("c", Json.Number(3))
            });
        }

        private static JsonArray MakeNestedArray()
        {
            return new JsonArray(new[] {
                new JsonArray(new[] {
                    Json.Number(1),
                    Json.Number(2),
                    Json.Number(3)
                }),
                new JsonArray(new[] {
                    Json.Number(4),
                    Json.Number(5),
                    Json.Number(6)
                }),
                new JsonArray(new[] {
                    Json.Number(7),
                    Json.Number(8),
                    Json.Number(9)
                })
            });
        }
    }
}
