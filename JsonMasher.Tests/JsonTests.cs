using System.Collections.Generic;
using System.Linq;
using Shouldly;
using JsonMasher.JsonRepresentation;
using Xunit;

namespace JsonMasher.Tests
{
    public class JsonTests
    {
        private record TestItem(string Input, string ExpectedOutput);

        public static IEnumerable<object[]> TestData
            => GetTestData().Select((System.Func<TestItem, object[]>)(item => (new object[] {
                item.Input,
                item.ExpectedOutput,
            })));

        [Theory]
        [MemberData(nameof(TestData))]
        public void EmptySequence(string input, string expectedOutput)
        {
            // Arrange
            Json data = input.AsJson();

            // Act
            var result = data.ToString();

            // Assert
            result.ShouldBe(expectedOutput);
        }

        private static IEnumerable<TestItem> GetTestData()
            => Enumerable.Empty<TestItem>()
                .Concat(SimpleExamples());

        private static IEnumerable<TestItem> SimpleExamples()
        {
            yield return new TestItem("[1, [2, 3], 4]", "[1, [2, 3], 4]");
            yield return new TestItem("{\"a\": 1}", "{\"a\": 1}");
            yield return new TestItem("{\"a\": []}", "{\"a\": []}");
            yield return new TestItem("{\"a\": [1, 2, true]}", "{\"a\": [1, 2, true]}");
            yield return new TestItem(
                "{\"a\": [null, true, false]}", "{\"a\": [null, true, false]}");
            yield return new TestItem("\"test\"", "\"test\"");
            yield return new TestItem("[12.3, 12.401]", "[12.3, 12.401]");
        }
    }
}
