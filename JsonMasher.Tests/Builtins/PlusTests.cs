using Shouldly;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Primitives;
using Xunit;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Tests.Builtins
{
    public class PlusTests
    {
        [Fact]
        public void Numbers()
        {
            // Arrange
            var data = Json.Number(1);
            var op = new FunctionCall(Plus.Builtin, new Identity(), new Identity());

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(2))
                .ShouldBe(true);
        }

        [Fact]
        public void OuterIterateNumbers()
        {
            // Arrange
            var data = Utils.JsonNumberArray(1, 2, 3);
            var op = Compose.AllParams(
                new Enumerate(),
                new FunctionCall(Plus.Builtin, new Identity(), new Identity()));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(2, 4, 6))
                .ShouldBe(true);
        }

        [Fact]
        public void InnerIterateNumbers()
        {
            // Arrange
            var data = Utils.JsonNumberArray(1, 2, 3);
            var op = new FunctionCall(Plus.Builtin, new Enumerate(), new Enumerate());

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(2, 3, 4, 3, 4, 5, 4, 5, 6))
                .ShouldBe(true);
        }

        [Fact]
        public void Arrays()
        {
            // Arrange
            var data = Json.ArrayParams(Json.Number(1));
            var op = new FunctionCall(Plus.Builtin, new Identity(), new Identity());

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Json.ArrayParams(Utils.JsonNumberArray(1, 1)))
                .ShouldBe(true);
        }

        [Fact]
        public void Strings()
        {
            // Arrange
            var data = Json.String("a");
            var op = new FunctionCall(Plus.Builtin, new Identity(), new Identity());

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Json.ArrayParams(Json.String("aa")))
                .ShouldBe(true);
        }

        [Fact]
        public void ObjectsWithLiteral()
        {
            // Arrange
            var data = Json.ObjectParams(new JsonProperty("a", Json.Number(1)));
            var op = new FunctionCall(
                Plus.Builtin,
                new Identity(),
                new Literal { Value = Json.ObjectParams(new JsonProperty("b", Json.Number(2))) });

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            var expectedValue = Json.ArrayParams(Json.ObjectParams(
                new JsonProperty("a", Json.Number(1)),
                new JsonProperty("b", Json.Number(2))));
            Json.Array(result).DeepEqual(expectedValue).ShouldBe(true);
        }
    }
}
