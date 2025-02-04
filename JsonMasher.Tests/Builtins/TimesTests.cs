using Shouldly;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Primitives;
using Xunit;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Tests.Builtins
{
    public class TimesTests
    {
        [Fact]
        public void Numbers()
        {
            // Arrange
            var data = Json.Null;
            var op = new FunctionCall(Times.Builtin, new Literal(2), new Literal(3));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(6))
                .ShouldBe(true);
        }

        [Fact]
        public void StringsAndNumbers()
        {
            // Arrange
            var data = Json.Null;
            var op = new FunctionCall(Times.Builtin, new Literal("ab"), new Literal(3));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Json.ArrayParams(Json.String("ababab")))
                .ShouldBe(true);
        }

        [Fact]
        public void NumbersAndStrings()
        {
            // Arrange
            var data = Json.Null;
            var op = new FunctionCall(Times.Builtin, new Literal(3), new Literal("ba"));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Json.ArrayParams(Json.String("bababa")))
                .ShouldBe(true);
        }
    }
}
