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
            var op = FunctionCall.Builtin(
                EqualsEquals.Builtin,
                new Literal { Value = Json.Number(2) },
                new Literal { Value = Json.Number(1) });

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
            var op = FunctionCall.Builtin(
                EqualsEquals.Builtin,
                new Literal { Value = Json.Number(2) },
                new Literal { Value = Json.Number(2) });

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            result.Should().BeEquivalentTo(Json.True);
        }
    }
}
