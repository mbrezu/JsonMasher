using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JsonMasher.Compiler;
using JsonMasher.Mashers;
using Xunit;

namespace JsonMasher.Tests.EndToEnd
{
    public class TicklimitTests
    {
        private record TestItem(string Program, int TickLimit);

        public static IEnumerable<object[]> TestData
            => GetTestData().Select((System.Func<TestItem, object[]>)(item => (new object[] {
                item.Program, item.TickLimit })));

        [Theory]
        [MemberData(nameof(TestData))]
        public void ProgramTest(string program, int tickLimit)
        {
            // Arrange
            var parser = new Parser();
            var (filter, _) = parser.Parse(program, new SequenceGenerator());
            var input = "null".AsJson().AsEnumerable();

            // Act
            Action action = () => {
                var (result, _) = new Mashers.JsonMasher().Mash(
                    input, filter, DefaultMashStack.Instance, null, tickLimit);
                result.ToList();
            };

            // Assert
            action
                .Should().Throw<JsonMasherException>()
                .Where(e => e.Message == $"Failed to complete in {tickLimit} ticks.");
        }

        private static IEnumerable<TestItem> GetTestData()
            => Enumerable.Empty<TestItem>()
                .Concat(SimplePrograms());

        private static IEnumerable<TestItem> SimplePrograms()
        {
            yield return new TestItem("range(1000)", 100);
            yield return new TestItem("range(1; 1000)", 100);
            yield return new TestItem("range(1; 1000; 1)", 100);
        }
    }
}
