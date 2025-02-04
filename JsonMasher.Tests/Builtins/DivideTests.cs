using Shouldly;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Primitives;
using Xunit;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Tests.Builtins
{
    public class DivideTests
    {
        [Fact]
        public void Numbers()
        {
            // Arrange
            var data = Json.Null;
            var op = new FunctionCall(Divide.Builtin, new Literal(1), new Literal(2));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(1.0/2))
                .ShouldBe(true);
        }
    }
}
