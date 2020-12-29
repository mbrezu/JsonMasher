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
            var op = new FunctionCall(EqualsEquals.Builtin, new Literal(2), new Literal(1));

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
            var op = new FunctionCall(EqualsEquals.Builtin, new Literal(2), new Literal(2));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Should().BeEquivalentTo(Json.True);
        }
    }
}
