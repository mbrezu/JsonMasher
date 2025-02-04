using System.Collections.Generic;
using JsonMasher.Compiler;
using Shouldly;
using Xunit;

namespace JsonMasher.Tests.Compiler
{
    public class PositionHighlighterTests
    {
        [Fact]
        public void ProgramWithLinesTests()
        {
            // Arrange
            var programWithLines = new ProgramWithLines(
                @"line1
line2

line4
line5".CleanCR()
            );

            // Act

            // Assert
            programWithLines.GetLine(0).ShouldBe("line1");
            programWithLines.GetLine(1).ShouldBe("line2");
            programWithLines.GetLine(3).ShouldBe("line4");
            programWithLines.GetLine(4).ShouldBe("line5");
            programWithLines.GetLineNumber(3).ShouldBe(0);
            programWithLines.GetLineNumber(7).ShouldBe(1);
            programWithLines.GetLineNumber(100).ShouldBe(4);
            programWithLines.GetColumnNumber(2).ShouldBe(2);
            programWithLines.GetColumnNumber(7).ShouldBe(1);
            programWithLines.GetColumnNumber(9).ShouldBe(3);
            programWithLines
                .GetHighlights(1, 3)
                .ShouldBe(new List<Highlight> { new Highlight(0, "line1", 1, 3) });
            programWithLines
                .GetHighlights(2, 20)
                .ShouldBe(
                    new List<Highlight>
                    {
                        new Highlight(0, "line1", 2, 5),
                        new Highlight(1, "line2", 0, 5),
                        new Highlight(2, "", 0, 0),
                        new Highlight(3, "line4", 0, 5),
                        new Highlight(4, "line5", 0, 1),
                    }
                );
        }

        [Fact]
        public void ProgramHighlighterTests()
        {
            // Arrange

            // Act
            var result = PositionHighlighter.Highlight(
                @"line1
line2

line4
line5".CleanCR(),
                2,
                20
            );

            // Assert
            result
                .CleanCR()
                .ShouldBe(
                    @"Line 1: line1
Line 1:   ^^^
Line 2: line2
Line 2: ^^^^^
Line 4: line4
Line 4: ^^^^^
Line 5: line5
Line 5: ^
".CleanCR()
                );
        }
    }
}
