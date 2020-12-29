using FluentAssertions;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Operators;
using JsonMasher.Mashers.Primitives;
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
            var op = new FunctionCall(
                Times.Builtin,
                new Literal { Value = Json.Number(2) },
                new Literal { Value = Json.Number(3) });

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
            var op = new FunctionCall(
                Times.Builtin,
                new Literal { Value = Json.String("ab") },
                new Literal { Value = Json.Number(3) });

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
            var op = new FunctionCall(
                Times.Builtin,
                new Literal { Value = Json.Number(3) },
                new Literal { Value = Json.String("ba") });

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Json.ArrayParams(Json.String("bababa")))
                .Should().BeTrue();
        }
    }
}
