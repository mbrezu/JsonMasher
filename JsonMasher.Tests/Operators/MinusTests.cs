using FluentAssertions;
using JsonMasher.Combinators;
using JsonMasher.Functions;
using JsonMasher.Primitives;
using Xunit;

namespace JsonMasher.Tests.Operators
{
    public class MinusTests
    {
        [Fact]
        public void Numbers()
        {
            // Arrange
            var data = Json.Null;
            var op = new BinaryOperator {
                First = new Literal { Value = Json.Number(2) },
                Second = new Literal { Value = Json.Number(1) },
                Function = Minus.Function
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual(Utils.JsonNumberArray(1)).Should().BeTrue();
        }

        [Fact]
        public void Arrays()
        {
            // Arrange
            var data = Json.Null;
            var op = new Compose {
                First = new BinaryOperator {
                    First = new Literal { 
                        Value = Utils.JsonNumberArray(1, 2, 3)
                    },
                    Second = new Literal {
                        Value = Utils.JsonNumberArray(3, 4, 5)
                    },
                    Function = Minus.Function
                },
                Second = Enumerate.Instance
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(1, 2))
                .Should().BeTrue();
        }
    }
}
