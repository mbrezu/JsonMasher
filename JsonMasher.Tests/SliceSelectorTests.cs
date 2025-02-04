using Shouldly;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Primitives;
using Xunit;

namespace JsonMasher.Tests
{
    public class SliceSelectorTests
    {
        [Fact]
        public void SliceSelectorFull()
        {
            // Arrange
            var data = "[1, 2, 3]".AsJson();
            var op = new SliceSelector {
                From = new Literal(1),
                To = new Literal(3)
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[[2, 3]]".AsJson())
                .ShouldBe(true);
        }

        [Fact]
        public void SliceSelectorFullNegative()
        {
            // Arrange
            var data = "[1, 2, 3]".AsJson();
            var op = new SliceSelector {
                From = new Literal(-3),
                To = new Literal(-1)
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[[1, 2]]".AsJson())
                .ShouldBe(true);
        }

        [Fact]
        public void SliceSelectorOptional()
        {
            // Arrange
            var data = "{}".AsJson();
            var op = new SliceSelector {
                From = new Literal(1),
                To = new Literal(3),
                IsOptional = true
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[]".AsJson())
                .ShouldBe(true);
        }

        [Fact]
        public void SliceSelectorOnlyFrom()
        {
            // Arrange
            var data = "[1, 2, 3]".AsJson();
            var op = new SliceSelector {
                From = new Literal(1),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[[2, 3]]".AsJson())
                .ShouldBe(true);
        }

        [Fact]
        public void SliceSelectorOnlyTo()
        {
            // Arrange
            var data = "[1, 2, 3]".AsJson();
            var op = new SliceSelector {
                From = new Literal(1),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[[2, 3]]".AsJson())
                .ShouldBe(true);
        }

        [Fact]
        public void SliceSelectorOnlyFromNegative()
        {
            // Arrange
            var data = "[1, 2, 3]".AsJson();
            var op = new SliceSelector {
                From = new Literal(-2),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[[2, 3]]".AsJson())
                .ShouldBe(true);
        }

        [Fact]
        public void SliceSelectorOnlyToNegative()
        {
            // Arrange
            var data = "[1, 2, 3]".AsJson();
            var op = new SliceSelector {
                To = new Literal(-2),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[[1]]".AsJson())
                .ShouldBe(true);
        }
    }
}