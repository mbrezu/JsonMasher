using FluentAssertions;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Primitives;
using Xunit;

namespace JsonMasher.Tests
{
    public class RegexTests
    {
        [Fact]
        public void GlobalMatch()
        {
            // Arrange
            var data = Json.String("this is the test string");
            var op = new FunctionCall(
                RegexMatch.Builtin, 
                new Literal("th"), 
                new Literal("g"), 
                new Literal { Value = Json.False });

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            var expectedJson = @"
[{""offset"":0,""length"":2,""string"":""th"",""captures"":[]},
 {""offset"":8,""length"":2,""string"":""th"",""captures"":[]}]";
            result.DeepEqual(expectedJson.AsJson()).Should().BeTrue();
        }

        [Fact]
        public void Captures()
        {
            // Arrange
            var data = Json.String("this is the test string");
            var op = new FunctionCall(
                RegexMatch.Builtin, 
                new Literal("(t)(h)"), 
                new Literal { Value = Json.Null }, 
                new Literal { Value = Json.False });

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            var expectedJson = @"
[{
  ""offset"": 0,
  ""length"": 2,
  ""string"": ""th"",
  ""captures"": [
    { ""offset"": 0, ""length"": 1, ""string"": ""t"", ""name"": ""1"" },
    { ""offset"": 1, ""length"": 1, ""string"": ""h"", ""name"": ""2"" }
  ]
}]";
            result.DeepEqual(expectedJson.AsJson()).Should().BeTrue();
        }

        [Fact]
        public void SingleMatch()
        {
            // Arrange
            var data = Json.String("this is the test string");
            var op = new FunctionCall(
                RegexMatch.Builtin, 
                new Literal("th"), 
                new Literal { Value = Json.Null }, 
                new Literal { Value = Json.False });

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            var expectedJson = @"
[{""offset"":0,""length"":2,""string"":""th"",""captures"":[]}]";
            result.DeepEqual(expectedJson.AsJson()).Should().BeTrue();
        }

        [Fact]
        public void JustTest()
        {
            // Arrange
            var data = Json.String("this is the test string");
            var op = new FunctionCall(
                RegexMatch.Builtin, 
                new Literal("th"), 
                new Literal { Value = Json.Null }, 
                new Literal { Value = Json.True });

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("true".AsJson()).Should().BeTrue();
        }

        [Fact]
        public void CaseInsensitive()
        {
            // Arrange
            var data = Json.String("THis");
            var op = new FunctionCall(
                RegexMatch.Builtin, 
                new Literal("th"), 
                new Literal("i"), 
                new Literal { Value = Json.False });

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            var expectedJson = @"
[{""offset"":0,""length"":2,""string"":""TH"",""captures"":[]}]";
            result.DeepEqual(expectedJson.AsJson()).Should().BeTrue();
        }

        [Fact]
        public void MultiLine()
        {
            // Arrange
            var data = Json.String("TH\nis");
            var op = new FunctionCall(
                RegexMatch.Builtin, 
                new Literal("h.i"), 
                new Literal("im"), 
                new Literal { Value = Json.False });

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            var expectedJson = "[{\"offset\":1,\"length\":3,\"string\":\"H\\ni\",\"captures\":[]}]";
            result.DeepEqual(expectedJson.AsJson()).Should().BeTrue();
        }

        [Fact]
        public void FilterEmptyMatches()
        {
            // Arrange
            var data = Json.String("this");
            var op = new FunctionCall(
                RegexMatch.Builtin, 
                new Literal(""), 
                new Literal("n"), 
                new Literal { Value = Json.False });

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[]".AsJson()).Should().BeTrue();
        }

        [Fact]
        public void Singleline()
        {
            // Arrange
            var data = Json.String("th\nis");
            var op = new FunctionCall(
                RegexMatch.Builtin, 
                new Literal("th$"), 
                new Literal("s"), 
                new Literal { Value = Json.False });

            // Act
            var result = op.RunAsSequence(data);

            // Assert
            Json.Array(result).DeepEqual("[]".AsJson()).Should().BeTrue();
        }

        [Fact]
        public void NotSingleline()
        {
            // Arrange
            var data = Json.String("th\nis");
            var op = new FunctionCall(
                RegexMatch.Builtin, 
                new Literal("th$"), 
                new Literal(""), 
                new Literal { Value = Json.False });

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            var expectedJson = "[{\"captures\": [], \"length\": 2, \"offset\": 0, \"string\": \"th\"}]";
            result.DeepEqual(expectedJson.AsJson()).Should().BeTrue();
        }

        [Fact]
        public void IgnoreWhiteSpace()
        {
            // Arrange
            var data = Json.String("this");
            var op = new FunctionCall(
                RegexMatch.Builtin, 
                new Literal("th is"), 
                new Literal("x"), 
                new Literal { Value = Json.False });

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            var expectedJson = "[{\"offset\":0,\"length\":4,\"string\":\"this\",\"captures\":[]}]";
            result.DeepEqual(expectedJson.AsJson()).Should().BeTrue();
        }
    }
}
