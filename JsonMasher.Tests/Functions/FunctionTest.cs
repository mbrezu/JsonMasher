using System.Collections.Generic;
using FluentAssertions;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Primitives;
using Xunit;

namespace JsonMasher.Tests.Functions
{
    public class FunctionTest
    {
        [Fact]
        public void NoArguments()
        {
            // Arrange
            var data = Utils.JsonNumberArray(1, 2, 3);
            var op = Compose.AllParams(
                new FunctionDefinition { 
                    Name = "test",
                    Arguments = new List<string>(),
                    Body = new FunctionCall(
                        Plus.Builtin, Identity.Instance, new Literal { Value = Json.Number(2) })
                },
                Enumerate.Instance,
                FunctionCall.ZeroArity("test"));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result)
                .DeepEqual(Utils.JsonNumberArray(3, 4, 5))
                .Should().BeTrue();
        }

        [Fact]
        public void WithArguments()
        {
            // Arrange
            var data = Utils.JsonNumberArray(1, 2, 3);
            var op = Compose.AllParams(
                new FunctionDefinition { 
                    Name = "x",
                    Arguments = new List<string>() {
                        "test1",
                        "test2"
                    },
                    Body = new ConstructArray {
                        Elements = Concat.AllParams(
                            FunctionCall.ZeroArity("test1"),
                            FunctionCall.ZeroArity("test2"))
                    }
                },
                new FunctionCall(new FunctionName("x", 2), Enumerate.Instance, Identity.Instance));

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            var expectedValues = Json.ArrayParams(
                Json.Number(1),
                Json.Number(2),
                Json.Number(3),
                Utils.JsonNumberArray(1, 2, 3));
            result.DeepEqual(expectedValues).Should().BeTrue();
        }
    }
}
