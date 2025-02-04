using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Shouldly;
using JsonMasher.JsonRepresentation;
using Xunit;
using JsonProperty = JsonMasher.JsonRepresentation.JsonProperty;

namespace JsonMasher.Tests
{
    public class JsonElementConversionTests
    {
        private record TestItem(string jsonStr, Json expectation);

        public static IEnumerable<object[]> TestData
            => GetTestData().Select(item => new object[] { item.jsonStr, item.expectation });

        [Theory]
        [MemberData(nameof(TestData))]
        public void TestDeepEqual(string jsonStr, Json expectation)
        {
            // Arrange
            var jsonElement = JsonDocument.Parse(jsonStr);

            // Act
            var result = jsonElement.AsJson();

            // Assert
            result.DeepEqual(expectation).ShouldBe(true);
        }

        private static IEnumerable<TestItem> GetTestData()
            => BasicTests()
                .Concat(ArrayTests())
                .Concat(ObjectTests());

        private static IEnumerable<TestItem> BasicTests()
        {
            yield return new TestItem("1", Json.Number(1));
            yield return new TestItem("\"a\"", Json.String("a"));
            yield return new TestItem("null", Json.Null);
            yield return new TestItem("true", Json.True);
            yield return new TestItem("false", Json.False);
        }

        private static IEnumerable<TestItem> ArrayTests()
        {
            yield return new TestItem("[]", Json.ArrayParams());
            yield return new TestItem("[1]", Json.ArrayParams(Json.Number(1)));
            yield return new TestItem(
                "[1, \"a\"]",
                Json.ArrayParams(Json.Number(1), Json.String("a")));
            yield return new TestItem(
                "[1, \"a\", true]",
                Json.ArrayParams(Json.Number(1), Json.String("a"), Json.True));
        }

        private static IEnumerable<TestItem> ObjectTests()
        {
            yield return new TestItem("{}", Json.ObjectParams());
            yield return new TestItem(
                "{ \"a\": 1}",
                Json.ObjectParams(
                    new JsonProperty("a", Json.Number(1))));
            yield return new TestItem(
                "{ \"a\": \"b\"}",
                Json.ObjectParams(
                    new JsonProperty("a", Json.String("b"))));
            yield return new TestItem(
                "{ \"a\": \"b\", \"c\": 2}",
                Json.ObjectParams(
                    new JsonProperty("a", Json.String("b")),
                    new JsonProperty("c", Json.Number(2))));
            yield return new TestItem(
                "{ \"a\": \"b\", \"c\": 2, \"d\": [1, true, null]}",
                Json.ObjectParams(
                    new JsonProperty("a", Json.String("b")),
                    new JsonProperty("c", Json.Number(2)),
                    new JsonProperty("d", Json.ArrayParams(
                        Json.Number(1),
                        Json.True,
                        Json.Null))));
        }
    }
}
