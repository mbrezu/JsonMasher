using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Combinators;
using JsonMasher.Functions;
using JsonMasher.Primitives;
using Xunit;

namespace JsonMasher.Tests.Functions
{
    public class FunctionTest
    {
        [Fact]
        public void NoArguments()
        {
            // Arrange
            var data = Json.ArrayParams(
                Json.Number(1),
                Json.Number(2),
                Json.Number(3));
            var op = Compose.AllParams(
                new FunctionDefinition { 
                    Name = "test",
                    Arguments = new List<string>(),
                    Body = new BinaryOperator {
                        First = Identity.Instance,
                        Second = new Literal { Value = Json.Number(2) },
                        Function = Plus.Function
                    }
                },
                Enumerate.Instance,
                FunctionCall.ZeroArity("test"));

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            var expectedValues = new List<double> { 3, 4, 5 };
            result.Select(x => x.GetNumber()).Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void WithArguments()
        {
            // Arrange
            var data = Json.ArrayParams(
                Json.Number(1),
                Json.Number(2),
                Json.Number(3));
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
                new FunctionCall {
                    Name = "x",
                    Arguments = new List<IJsonMasherOperator>() {
                        Enumerate.Instance,
                        Identity.Instance
                    },
                });

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            var expectedValues = Json.ArrayParams(
                Json.Number(1),
                Json.Number(2),
                Json.Number(3),
                Json.ArrayParams(
                    Json.Number(1),
                    Json.Number(2),
                    Json.Number(3)
                ));
            result.DeepEqual(expectedValues).Should().BeTrue();
        }
    }
}
