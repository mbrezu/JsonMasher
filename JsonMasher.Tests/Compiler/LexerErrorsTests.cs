using System;
using System.Linq;
using FluentAssertions;
using JsonMasher.Compiler;
using Xunit;

namespace JsonMasher.Tests.Compiler
{
    public class LexerErrorsTests
    {
        [Fact]
        public void UnexpectedToken()
        {
            // Arrange
            var program = "def map(x): .[] @";
            var lexer = new Lexer();
            var expectedHighlights = @"Line 1: def map(x): .[] @
Line 1:                 ^
";

            // Act
            Action action = () => lexer.Tokenize(program).ToList();

            // Assert
            action
                .Should().Throw<JsonMasherException>()
                .Where(e => VerifyException(
                    e, Messages.Lexer.UnexpectedCharacter, 1, 17, expectedHighlights));
        }

        [Fact]
        public void UnexpectedEndOfInputInString()
        {
            // Arrange
            var program = @"
""abc";
            var lexer = new Lexer();
            var expectedHighlights = @"Line 2: ""abc
Line 2:     ^
";

            // Act
            Action action = () => lexer.Tokenize(program).ToList();

            // Assert
            action
                .Should().Throw<JsonMasherException>()
                .Where(e => VerifyException(
                    e, Messages.Lexer.EoiInsideString, 2, 5, expectedHighlights));
        }

        [Fact]
        public void UnexpectedEscapeSequence()
        {
            // Arrange
            var program = "\"blabla\\k\"";
            var lexer = new Lexer();
            var expectedHighlights = @"Line 1: ""blabla\k""
Line 1: ^^^^^^^^^^
";

            // Act
            Action action = () => lexer.Tokenize(program).ToList();

            // Assert
            action
                .Should().Throw<JsonMasherException>()
                .Where(e => VerifyException(
                    e, Messages.Lexer.InvalidEscapeSequence, 1, 1, expectedHighlights));
        }

        private bool VerifyException(
            JsonMasherException e, string message, int line, int column, string highlights)
        {
            e.Message.Should().Be(message);
            e.Line.Should().Be(line);
            e.Column.Should().Be(column);
            e.Highlights.CleanCR().Should().Be(highlights.CleanCR());
            return true;
        }
    }
}
