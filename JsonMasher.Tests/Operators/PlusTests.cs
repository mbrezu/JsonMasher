using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Combinators;
using JsonMasher.Functions;
using JsonMasher.Primitives;
using Xunit;

namespace JsonMasher.Tests.Operators
{
    public class PlusTests
    {
        [Fact]
        public void Numbers()
        {
            // Arrange
            var data = Json.Number(1);
            var op = new BinaryOperator {
                First = Identity.Instance,
                Second = Identity.Instance,
                Function = Plus.Function
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            var expectedValues = new List<double> { 2 };
            result.Select(x => x.GetNumber()).Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void OuterIterateNumbers()
        {
            // Arrange
            var data = Json.ArrayParams(Json.Number(1), Json.Number(2), Json.Number(3));
            var op = new Compose {
                First = Enumerate.Instance,
                Second = new BinaryOperator {
                    First = Identity.Instance,
                    Second = Identity.Instance,
                    Function = Plus.Function
            }};

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            var expectedValues = new List<double> { 2, 4, 6 };
            result.Select(x => x.GetNumber()).Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void InnerIterateNumbers()
        {
            // Arrange
            var data = Json.ArrayParams(Json.Number(1), Json.Number(2), Json.Number(3));
            var op = new BinaryOperator {
                First = Enumerate.Instance,
                Second = Enumerate.Instance,
                Function = Plus.Function
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            var expectedValues = new List<double> { 2, 3, 4, 3, 4, 5, 4, 5, 6};
            result.Select(x => x.GetNumber()).Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void Arrays()
        {
            // Arrange
            var data = Json.ArrayParams(Json.Number(1));
            var op = new BinaryOperator {
                First = Identity.Instance,
                Second = Identity.Instance,
                Function = Plus.Function
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            var expectedValue = Json.ArrayParams(Json.Number(1), Json.Number(2));
            result.Count().Should().Be(1);
            result.First().Should().BeEquivalentTo(expectedValue);
        }

        [Fact]
        public void Strings()
        {
            // Arrange
            var data = Json.String("a");
            var op = new BinaryOperator {
                First = Identity.Instance,
                Second = Identity.Instance,
                Function = Plus.Function
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            var expectedValue = Json.String("aa");
            result.Count().Should().Be(1);
            result.First().Should().BeEquivalentTo(expectedValue);
        }

        [Fact]
        public void ObjectsWithLiteral()
        {
            // Arrange
            var data = Json.ObjectParams(new JsonProperty("a", Json.Number(1)));
            var op = new BinaryOperator {
                First = Identity.Instance,
                Second = new Literal { Value = Json.ObjectParams(new JsonProperty("b", Json.Number(2)))},
                Function = Plus.Function
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            var expectedValue = Json.ObjectParams(
                new JsonProperty("a", Json.Number(1)),
                new JsonProperty("b", Json.Number(2)));
            result.Count().Should().Be(1);
            result.First().Should().BeEquivalentTo(expectedValue);
        }
    }
}
