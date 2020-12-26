using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Combinators;
using JsonMasher.Functions;
using JsonMasher.Primitives;
using Xunit;

namespace JsonMasher.Tests.Functions
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
            var expectedValues = new List<double> { 6 };
            result.Select(x => x.GetNumber()).Should().BeEquivalentTo(expectedValues);
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
            var expectedValues = new List<string> { "ababab" };
            result.Select(x => x.GetString()).Should().BeEquivalentTo(expectedValues);
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
            var expectedValues = new List<string> { "bababa" };
            result.Select(x => x.GetString()).Should().BeEquivalentTo(expectedValues);
        }
    }
}
