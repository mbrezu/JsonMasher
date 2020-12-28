using FluentAssertions;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Operators;
using JsonMasher.Mashers.Primitives;
using Xunit;

namespace JsonMasher.Tests.Operators
{
    public class RelationalTests
    {
        [Fact]
        public void EqualsEqualsTestFalse()
        {
            // Arrange
            var data = Json.Null;
            var op = new BinaryOperator {
                First = new Literal { Value = Json.Number(2) },
                Second = new Literal { Value = Json.Number(1) },
                Operator = EqualsEquals.Operator
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Should().BeEquivalentTo(Json.False);
        }

        [Fact]
        public void EqualsEqualsTestTrue()
        {
            // Arrange
            var data = Json.Null;
            var op = new BinaryOperator {
                First = new Literal { Value = Json.Number(2) },
                Second = new Literal { Value = Json.Number(2) },
                Operator = EqualsEquals.Operator
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Should().BeEquivalentTo(Json.True);
        }
    }
}
