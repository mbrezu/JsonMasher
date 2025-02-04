using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Primitives;
using Shouldly;
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
            var op = new Assignment
            {
                PathExpression = new Identity(),
                Masher = new FunctionCall(Plus.Builtin, new Identity(), new Literal(2)),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[4]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void ArrayEnumerationAssignment()
        {
            // Arrange
            var data = "[1, 2]".AsJson();
            var op = new Assignment
            {
                PathExpression = new Enumerate(),
                Masher = new FunctionCall(Plus.Builtin, new Identity(), new Literal(2)),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[[3, 4]]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void ArrayDoubleEnumerationAssignment()
        {
            // Arrange
            var data = "[[1, 2], [3, 4]]".AsJson();
            var op = new Assignment
            {
                PathExpression = Compose.AllParams(new Enumerate(), new Enumerate()),
                Masher = new FunctionCall(Plus.Builtin, new Identity(), new Literal(2)),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[[[3, 4], [5, 6]]]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void ObjectEnumerationAssignment()
        {
            // Arrange
            var data = "{ \"a\": 1, \"b\": 2 }".AsJson();
            var op = new Assignment
            {
                PathExpression = new Enumerate(),
                Masher = new FunctionCall(Plus.Builtin, new Identity(), new Literal(2)),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[{ \"a\": 3, \"b\": 4 }]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void ObjectKeyAssignment()
        {
            // Arrange
            var data = "{ \"a\": 1, \"b\": 2 }".AsJson();
            var op = new Assignment
            {
                PathExpression = new StringSelector { Key = "a" },
                Masher = new FunctionCall(Plus.Builtin, new Identity(), new Literal(2)),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[{ \"a\": 3, \"b\": 2 }]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void ObjectMultiKeyPathAssignment()
        {
            // Arrange
            var data = "{ \"a\": { \"c\": 1 }, \"b\": 2 }".AsJson();
            var op = new Assignment
            {
                PathExpression = Compose.AllParams(
                    new StringSelector { Key = "a" },
                    new StringSelector { Key = "c" }
                ),
                Masher = new FunctionCall(Plus.Builtin, new Identity(), new Literal(2)),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[{ \"a\": { \"c\": 3 }, \"b\": 2 }]".AsJson())
                .ShouldBe(true);
        }

        [Fact]
        public void ArrayIndexSelectorAssignment()
        {
            // Arrange
            var data = "[[1, 2], [3, 4]]".AsJson();
            var op = new Assignment
            {
                PathExpression = Compose.AllParams(
                    new Enumerate(),
                    new Selector { Index = new Literal(0) }
                ),
                Masher = new FunctionCall(Plus.Builtin, new Identity(), new Literal(2)),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[[[3, 2], [5, 4]]]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void ObjectIndexSelectorAssignment()
        {
            // Arrange
            var data = "[{ \"a\": 1, \"b\": 2 }, { \"a\": 3, \"b\": 4 }]".AsJson();
            var op = new Assignment
            {
                PathExpression = Compose.AllParams(
                    new Enumerate(),
                    new Selector { Index = new Literal { Value = Json.String("b") } }
                ),
                Masher = new FunctionCall(Plus.Builtin, new Identity(), new Literal(2)),
            };

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual("[[{ \"a\": 1, \"b\": 4 }, { \"a\": 3, \"b\": 6 }]]".AsJson())
                .ShouldBe(true);
        }
    }
}
