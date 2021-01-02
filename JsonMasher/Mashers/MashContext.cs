using System;
using System.Collections.Generic;
using System.Text;
using JsonMasher.Compiler;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers
{
    public class MashContext : IMashContext
    {

        List<Json> _log = new();
        MashEnvironment _env = new();
        int ticks = 0;
        public int TickLimit { get; init; }

        public IEnumerable<Json> Log => _log;
        public SourceInformation SourceInformation { get; init; }

        public MashContext()
        {
            PushEnvironmentFrame();
        }

        public void LogValue(Json value) => _log.Add(value);

        public void PushEnvironmentFrame() => _env = _env.Push();

        public void PopEnvironmentFrame() => _env = _env.Pop();

        public void SetVariable(string name, Json value) => _env.SetVariable(name, value);

        public Json GetVariable(string name, IMashStack stack) 
            => _env.GetVariable(name, stack, this);
        public void SetCallable(FunctionName name, Callable value) => _env.SetCallable(name, value);

        public Callable GetCallable(FunctionName name, IMashStack stack)
            => _env.GetCallable(name, stack, this);
        public void SetCallable(string name, Callable value) => _env.SetCallable(name, value);

        public Callable GetCallable(string name, IMashStack stack)
            => _env.GetCallable(name, stack, this);
        public Exception Error(string message, IMashStack stack, params Json[] values)
        {
            var programWithLines = SourceInformation != null 
                ? new ProgramWithLines(SourceInformation.Program)
                : null;
            var stackSb = new StringBuilder();
            foreach (var frame in stack.GetValues())
            {
                var position = SourceInformation?.GetProgramPosition(frame);
                if (position != null)
                {
                    stackSb.Append(PositionHighlighter.Highlight(
                        programWithLines, position.StartPosition, position.EndPosition));
                }
                else
                {
                    stackSb.AppendLine(frame.GetType().Name.ToString());
                }
            }
            var line = 0;
            var column = 0;
            var topFrame = stack.Top;
            if (topFrame != null) // TODO: should search the first frame with a position.
            {
                var position = SourceInformation?.GetProgramPosition(topFrame);
                if (position != null)
                {
                    line = programWithLines.GetLineNumber(position.StartPosition) + 1;
                    column = programWithLines.GetColumnNumber(position.StartPosition) + 1;
                }
            }
            return new JsonMasherException(
                message, line, column, stackSb.ToString(), null, values);
        }

        public void Tick(IMashStack stack)
        {
            if (TickLimit != 0)
            {
                ticks ++;
                if (ticks > TickLimit)
                {
                    throw Error($"Failed to complete in {TickLimit} ticks.", stack);
                }
            }
        }
    }
}
