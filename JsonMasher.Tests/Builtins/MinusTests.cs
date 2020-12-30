using FluentAssertions;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Primitives;
using Xunit;

namespace JsonMasher.Tests.Builtins
{
    public class MinusTests
    {
        [Fact]
        public void Numbers()
        {
            // Arrange
            var data = Json.Null;
            var op = new FunctionCall(
                Minus.Builtin,
                new Literal(2),
                new Literal(1));

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
                First = new FunctionCall(
                    Minus.Builtin,
                    new Literal { 
                        Value = Utils.JsonNumberArray(1, 2, 3)
                    },
                    new Literal {
                        Value = Utils.JsonNumberArray(3, 4, 5)
                    }),
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
