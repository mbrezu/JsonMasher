using FluentAssertions;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Operators;
using JsonMasher.Mashers.Primitives;
using Xunit;

namespace JsonMasher.Tests.Operators
{
    public class DivideTests
    {
        [Fact]
        public void Numbers()
        {
            // Arrange
            var data = Json.Null;
            var op = new BinaryOperator {
                First = new Literal { Value = Json.Number(1) },
                Second = new Literal { Value = Json.Number(2) },
                Operator = Divide.Operator
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(1.0/2))
                .Should().BeTrue();
        }
    }
}
