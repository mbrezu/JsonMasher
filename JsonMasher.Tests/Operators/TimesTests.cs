using FluentAssertions;
using JsonMasher.Combinators;
using JsonMasher.Functions;
using JsonMasher.Primitives;
using Xunit;

namespace JsonMasher.Tests.Operators
{
    public class TimesTests
    {
        [Fact]
        public void Numbers()
        {
            // Arrange
            var data = Json.Null;
            var op = new BinaryOperator {
                First = new Literal { Value = Json.Number(2) },
                Second = new Literal { Value = Json.Number(3) },
                Function = Times.Function
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(6))
                .Should().BeTrue();
        }

        [Fact]
        public void StringsAndNumbers()
        {
            // Arrange
            var data = Json.Null;
            var op = new BinaryOperator {
                First = new Literal { Value = Json.String("ab") },
                Second = new Literal { Value = Json.Number(3) },
                Function = Times.Function
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Json.ArrayParams(Json.String("ababab")))
                .Should().BeTrue();
        }

        [Fact]
        public void NumbersAndStrings()
        {
            // Arrange
            var data = Json.Null;
            var op = new BinaryOperator {
                First = new Literal { Value = Json.Number(3) },
                Second = new Literal { Value = Json.String("ba") },
                Function = Times.Function
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Json.ArrayParams(Json.String("bababa")))
                .Should().BeTrue();
        }
    }
}
