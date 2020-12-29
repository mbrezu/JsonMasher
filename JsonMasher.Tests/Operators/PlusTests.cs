using FluentAssertions;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Operators;
using JsonMasher.Mashers.Primitives;
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
            var op = FunctionCall.Builtin(Plus.Builtin, Identity.Instance, Identity.Instance);

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(2))
                .Should().BeTrue();
        }

        [Fact]
        public void OuterIterateNumbers()
        {
            // Arrange
            var data = Utils.JsonNumberArray(1, 2, 3);
            var op = Compose.AllParams(
                Enumerate.Instance,
                FunctionCall.Builtin(Plus.Builtin, Identity.Instance, Identity.Instance));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(2, 4, 6))
                .Should().BeTrue();
        }

        [Fact]
        public void InnerIterateNumbers()
        {
            // Arrange
            var data = Utils.JsonNumberArray(1, 2, 3);
            var op = FunctionCall.Builtin(Plus.Builtin, Enumerate.Instance, Enumerate.Instance);

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(2, 3, 4, 3, 4, 5, 4, 5, 6))
                .Should().BeTrue();
        }

        [Fact]
        public void Arrays()
        {
            // Arrange
            var data = Json.ArrayParams(Json.Number(1));
            var op = FunctionCall.Builtin(Plus.Builtin, Identity.Instance, Identity.Instance);

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Json.ArrayParams(Utils.JsonNumberArray(1, 1)))
                .Should().BeTrue();
        }

        [Fact]
        public void Strings()
        {
            // Arrange
            var data = Json.String("a");
            var op = FunctionCall.Builtin(Plus.Builtin, Identity.Instance, Identity.Instance);

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Json.ArrayParams(Json.String("aa")))
                .Should().BeTrue();
        }

        [Fact]
        public void ObjectsWithLiteral()
        {
            // Arrange
            var data = Json.ObjectParams(new JsonProperty("a", Json.Number(1)));
            var op = FunctionCall.Builtin(
                Plus.Builtin, 
                Identity.Instance, 
                new Literal { Value = Json.ObjectParams(new JsonProperty("b", Json.Number(2))) });

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            var expectedValue = Json.ArrayParams(Json.ObjectParams(
                new JsonProperty("a", Json.Number(1)),
                new JsonProperty("b", Json.Number(2))));
            Json.Array(result).DeepEqual(expectedValue).Should().BeTrue();
        }
    }
}
