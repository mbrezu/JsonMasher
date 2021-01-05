using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.JsonRepresentation;
using Xunit;

namespace JsonMasher.Tests
{
    public class DeepEqualTests
    {
        private record TestItem(Json d1, Json d2, bool areEqual);

        public static IEnumerable<object[]> TestData
            => GetTestData().Select(item => new object[] { item.d1, item.d2, item.areEqual });

        [Theory]
        [MemberData(nameof(TestData))]
        public void TestDeepEqual(Json d1, Json d2, bool areEqual)
        {
            // Arrange

            // Act
            var result = d1.DeepEqual(d2);

            // Assert
            result.Should().Be(areEqual);
        }
        private static IEnumerable<TestItem> GetTestData()
            => BasicTests()
                .Concat(ArrayTests())
                .Concat(ObjectTests());

        private static IEnumerable<TestItem> BasicTests()
        {
            yield return new TestItem(Json.Number(10), Json.Number(10), true);
            yield return new TestItem(Json.Number(10), Json.Number(20), false);
            yield return new TestItem(Json.Number(10), Json.String("10"), false);
            yield return new TestItem(Json.String("10"), Json.String("10"), true);
            yield return new TestItem(Json.String("10"), Json.String("20"), false);
            yield return new TestItem(Json.False, Json.False, true);
            yield return new TestItem(Json.False, Json.True, false);
            yield return new TestItem(Json.True, Json.True, true);
            yield return new TestItem(Json.True, Json.Null, false);
            yield return new TestItem(Json.Null, Json.Null, true);
            yield return new TestItem(Json.Null, Json.Undefined, false);
            yield return new TestItem(Json.Undefined, Json.Undefined, true);
            yield return new TestItem(Json.Undefined, Json.ObjectParams(), false);
            yield return new TestItem(Json.ObjectParams(), Json.ObjectParams(), true);
        }

        private static IEnumerable<TestItem> ArrayTests()
        {
            yield return new TestItem(Json.ArrayParams(), Json.ArrayParams(), true);
            yield return new TestItem(
                Json.ArrayParams(Json.Number(1)),
                Json.ArrayParams(Json.String("a")),
                false);
            yield return new TestItem(
                Json.ArrayParams(Json.String("a")),
                Json.ArrayParams(Json.String("a")),
                true);
            yield return new TestItem(
                Json.ArrayParams(Json.String("a"), Json.String("b")),
                Json.ArrayParams(Json.String("a")),
                false);
            yield return new TestItem(
                Json.ArrayParams(Json.String("a"), Json.String("b")),
                Json.ArrayParams(Json.String("a"), Json.String("b")),
                true);
        }

        private static IEnumerable<TestItem> ObjectTests()
        {
            yield return new TestItem(Json.ObjectParams(), Json.ObjectParams(), true);
            yield return new TestItem(
                Json.ObjectParams(),
                Json.ObjectParams(new JsonProperty("a", Json.Number(1))),
                false);
            yield return new TestItem(
                Json.ObjectParams(new JsonProperty("a", Json.Number(1))),
                Json.ObjectParams(new JsonProperty("a", Json.Number(1))),
                true);
            yield return new TestItem(
                Json.ObjectParams(new JsonProperty("a", Json.Number(1))),
                Json.ObjectParams(new JsonProperty("a", Json.String("a"))),
                false);
            yield return new TestItem(
                Json.ObjectParams(
                    new JsonProperty("a", Json.Number(1)),
                    new JsonProperty("b", Json.Number(2))),
                Json.ObjectParams(
                    new JsonProperty("a", Json.Number(1)),
                    new JsonProperty("b", Json.Number(2))),
                true);
            yield return new TestItem(
                Json.ObjectParams(
                    new JsonProperty("a", Json.Number(1))),
                Json.ObjectParams(
                    new JsonProperty("a", Json.Number(1)),
                    new JsonProperty("b", Json.Number(2))),
                false);
            yield return new TestItem(
                Json.ObjectParams(
                    new JsonProperty("a", Json.Number(1)),
                    new JsonProperty("b", Json.ArrayParams())),
                Json.ObjectParams(
                    new JsonProperty("a", Json.Number(1)),
                    new JsonProperty("b", Json.ArrayParams())),
                true);
            yield return new TestItem(
                Json.ObjectParams(
                    new JsonProperty("a", Json.Number(1)),
                    new JsonProperty("b", Json.ArrayParams(Json.String("a")))),
                Json.ObjectParams(
                    new JsonProperty("a", Json.Number(1)),
                    new JsonProperty("b", Json.ArrayParams())),
                false);
            yield return new TestItem(
                Json.ObjectParams(
                    new JsonProperty("a", Json.Number(1)),
                    new JsonProperty("b", Json.ArrayParams(Json.String("a")))),
                Json.ObjectParams(
                    new JsonProperty("a", Json.Number(1)),
                    new JsonProperty("b", Json.ArrayParams(Json.Number(1)))),
                false);
            yield return new TestItem(
                Json.ObjectParams(
                    new JsonProperty("a", Json.Number(1)),
                    new JsonProperty("b", Json.ArrayParams(Json.String("a")))),
                Json.ObjectParams(
                    new JsonProperty("a", Json.Number(1)),
                    new JsonProperty("b", Json.ArrayParams(Json.String("a")))),
                true);
        }
    }
}
