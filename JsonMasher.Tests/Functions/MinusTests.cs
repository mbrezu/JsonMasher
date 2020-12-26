using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Combinators;
using JsonMasher.Functions;
using JsonMasher.Primitives;
using Xunit;

namespace JsonMasher.Tests.Functions
{
    public class MinusTests
    {
        [Fact]
        public void Numbers()
        {
            // Arrange
            var data = Json.Null;
            var op = new BinaryOperator {
                First = new Literal { Value = Json.Number(2) },
                Second = new Literal { Value = Json.Number(1) },
                Function = Minus.Function
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            var expectedValues = new List<double> { 1 };
            result.Select(x => x.GetNumber()).Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void Arrays()
        {
            // Arrange
            var data = Json.Null;
            var op = new Compose {
                First = new BinaryOperator {
                    First = new Literal { 
                        Value = Json.ArrayParams(Json.Number(1), Json.Number(2), Json.Number(3)) 
                    },
                    Second = new Literal {
                        Value = Json.ArrayParams(Json.Number(3), Json.Number(4), Json.Number(5))
                    },
                    Function = Minus.Function
                },
                Second = Enumerate.Instance
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            var expectedValues = new List<double> { 1, 2 };
            result.Select(x => x.GetNumber()).Should().BeEquivalentTo(expectedValues);
        }
    }
}
