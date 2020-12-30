using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonMasher.Compiler
{
    public class ProgramWithLines
    {
        string _program;
        int[] _linesStart;
        
        public ProgramWithLines(string program)
        {
            _program = program;
            _linesStart = Process();
        }

        private int[] Process()
        {
            List<int> result = new();
            result.Add(0);
            for (int i = 0; i < _program.Length; i++)
            {
                if (_program[i] == '\n')
                {
                    result.Add(i+1);
                }
            }
            return result.ToArray();
        }

        public string GetLine(int lineNumber)
        {
            if (lineNumber < _linesStart.Length - 1)
            {
                var line = _program.Substring(
                    _linesStart[lineNumber], 
                    _linesStart[lineNumber + 1] - _linesStart[lineNumber] - 1);
                return line;
            }
            else
            {
                return _program.Substring(_linesStart[lineNumber]);
            }
        }

        public int GetLineNumber(int position)
        {
            if (position > _linesStart[_linesStart.Length - 1])
            {
                return _linesStart.Length - 1;
            }
            var pos = Array.BinarySearch(_linesStart, 0, _linesStart.Length, position);
            return pos >= 0 ? pos : (~pos - 1);
        }

        public int GetColumnNumber(int position) => position - _linesStart[GetLineNumber(position)];

        public IEnumerable<Highlight> GetHighlights(int startPosition, int endPosition)
        {
            var lineStart = GetLineNumber(startPosition);
            var lineEnd = GetLineNumber(endPosition);
            if (lineStart == lineEnd)
            {
                yield return new Highlight(
                    lineStart,
                    GetLine(lineStart),
                    GetColumnNumber(startPosition),
                    GetColumnNumber(endPosition));
            }
            else
            {
                for (int i = lineStart; i <= lineEnd; i++)
                {
                    var line = GetLine(i);
                    int columnStart = i == lineStart ? GetColumnNumber(startPosition) : 0;
                    int columnEnd = i == lineEnd ? GetColumnNumber(endPosition) : line.Length;
                    yield return new Highlight(i, line, columnStart, columnEnd);
                }
            }
        }
    }

    public record Highlight(int LineNumber, string Line, int ColumnStart, int ColumnEnd);

    public class PositionHighlighter
    {
        public static string Highlight(string program, int startPosition, int endPosition)
        {
            var programWithLines = new ProgramWithLines(program);
            return Highlight(programWithLines, startPosition, endPosition);
        }

        public static string Highlight(ProgramWithLines programWithLines, int startPosition, int endPosition)
        {
            var highlights = programWithLines.GetHighlights(startPosition, endPosition);
            var result = new StringBuilder();
            foreach (var highlight in highlights.Where(h => h.ColumnEnd - h.ColumnStart > 0))
            {
                result.AppendLine($"Line {highlight.LineNumber + 1}: {highlight.Line}");
                result.AppendLine(
                    $"Line {highlight.LineNumber + 1}: {ColumnMarkers(highlight)}");
            }
            return result.ToString();
        }

        private static string ColumnMarkers(Highlight highlight) 
            => " ".Repeat(highlight.ColumnStart) 
                + "^".Repeat(highlight.ColumnEnd - highlight.ColumnStart);
    }
}
