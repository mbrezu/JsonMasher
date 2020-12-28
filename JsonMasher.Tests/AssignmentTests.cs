using FluentAssertions;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Operators;
using JsonMasher.Mashers.Primitives;
using Xunit;

namespace JsonMasher.Tests
{
    public class AssignmentTests
    {
        [Fact]
        public void SimpleAssignment()
        {
            // Arrange
            var data = Json.Number(2);
            var op = new PipeAssignment {
                PathExpression = Identity.Instance,
                Masher = new BinaryOperator {
                    First = Identity.Instance,
                    Second = new Literal { Value = Json.Number(2) },
                    Operator = Plus.Operator
                }
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[4]".AsJson())
                .Should().BeTrue();
        }

        [Fact]
        public void ArrayEnumerationAssignment()
        {
            // Arrange
            var data = "[1, 2]".AsJson();
            var op = new PipeAssignment {
                PathExpression = Enumerate.Instance,
                Masher = new BinaryOperator {
                    First = Identity.Instance,
                    Second = new Literal { Value = Json.Number(2) },
                    Operator = Plus.Operator
                }
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[[3, 4]]".AsJson())
                .Should().BeTrue();
        }

        [Fact]
        public void ArrayDoubleEnumerationAssignment()
        {
            // Arrange
            var data = "[[1, 2], [3, 4]]".AsJson();
            var op = new PipeAssignment {
                PathExpression = Compose.AllParams(
                    Enumerate.Instance,
                    Enumerate.Instance),
                Masher = new BinaryOperator {
                    First = Identity.Instance,
                    Second = new Literal { Value = Json.Number(2) },
                    Operator = Plus.Operator
                }
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[[[3, 4], [5, 6]]]".AsJson())
                .Should().BeTrue();
        }

        [Fact]
        public void ObjectEnumerationAssignment()
        {
            // Arrange
            var data = "{ \"a\": 1, \"b\": 2 }".AsJson();
            var op = new PipeAssignment {
                PathExpression = Enumerate.Instance,
                Masher = new BinaryOperator {
                    First = Identity.Instance,
                    Second = new Literal { Value = Json.Number(2) },
                    Operator = Plus.Operator
                }
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[{ \"a\": 3, \"b\": 4 }]".AsJson())
                .Should().BeTrue();
        }

        [Fact]
        public void ObjectKeyAssignment()
        {
            // Arrange
            var data = "{ \"a\": 1, \"b\": 2 }".AsJson();
            var op = new PipeAssignment {
                PathExpression = new StringSelector { Key = "a"},
                Masher = new BinaryOperator {
                    First = Identity.Instance,
                    Second = new Literal { Value = Json.Number(2) },
                    Operator = Plus.Operator
                }
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[{ \"a\": 3, \"b\": 2 }]".AsJson())
                .Should().BeTrue();
        }

        [Fact]
        public void ObjectMultiKeyPathAssignment()
        {
            // Arrange
            var data = "{ \"a\": { \"c\": 1 }, \"b\": 2 }".AsJson();
            var op = new PipeAssignment {
                PathExpression = Compose.AllParams(
                    new StringSelector { Key = "a" },
                    new StringSelector { Key = "c" }),
                Masher = new BinaryOperator {
                    First = Identity.Instance,
                    Second = new Literal { Value = Json.Number(2) },
                    Operator = Plus.Operator
                }
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[{ \"a\": { \"c\": 3 }, \"b\": 2 }]".AsJson())
                .Should().BeTrue();
        }

        [Fact]
        public void ArrayIndexSelectorAssignment()
        {
            // Arrange
            var data = "[[1, 2], [3, 4]]".AsJson();
            var op = new PipeAssignment {
                PathExpression = Compose.AllParams(
                    Enumerate.Instance,
                    new Selector { Index = new Literal { Value = Json.Number(0) } }),
                Masher = new BinaryOperator {
                    First = Identity.Instance,
                    Second = new Literal { Value = Json.Number(2) },
                    Operator = Plus.Operator
                }
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[[[3, 2], [5, 4]]]".AsJson())
                .Should().BeTrue();
        }
    }
}
