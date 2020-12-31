using System;
using System.Collections.Generic;
using System.Text;
using JsonMasher.Compiler;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers
{
    public class MashContext : IMashContext
    {
        record Frame(
            Dictionary<string, Json> Variables,
            Dictionary<FunctionName, Callable> Callables,
            Dictionary<string, Callable> ZeroArityCallables);

        List<Json> _log = new();
        List<Frame> _env = new();
        int ticks = 0;
        public int TickLimit { get; init; }

        public IEnumerable<Json> Log => _log;
        public SourceInformation SourceInformation { get; init; }

        public MashContext()
        {
            PushEnvironmentFrame();
        }

        public void LogValue(Json value)
            => _log.Add(value);

        public void PushEnvironmentFrame()
            => _env.Add(new Frame(
                new Dictionary<string, Json>(),
                new Dictionary<FunctionName, Callable>(),
                new Dictionary<string, Callable>()));

        public void PopEnvironmentFrame()
            => _env.RemoveAt(_env.Count - 1);

        public void SetVariable(string name, Json value)
            => _env[_env.Count - 1].Variables[name] = value;

        public Json GetVariable(string name, IMashStack stack)
        {
            for (int i = _env.Count - 1; i > -1; i--)
            {
                var frame = _env[i];
                if (frame.Variables.ContainsKey(name))
                {
                    return frame.Variables[name];
                }
            }
            throw Error($"Cannot find variable ${name}.", stack);
        }

        public void SetCallable(FunctionName name, Callable value)
        {
            if (name.Arity == 0)
            {
                SetCallable(name.Name, value);
            }
            else
            {
                _env[_env.Count - 1].Callables[name] = value;
            }
        }

        public Callable GetCallable(FunctionName name, IMashStack stack)
        {
            if (name.Arity == 0)
            {
                return GetCallable(name.Name, stack);
            }
            else
            {
                for (int i = _env.Count - 1; i > -1; i--)
                {
                    var frame = _env[i];
                    if (frame.Callables.ContainsKey(name))
                    {
                        return frame.Callables[name];
                    }
                }
                throw Error($"Function {name.Name}/{name.Arity} is not known.", stack);
            }
        }

        public void SetCallable(string name, Callable value)
        {
            _env[_env.Count - 1].ZeroArityCallables[name] = value;
        }

        public Callable GetCallable(string name, IMashStack stack)
        {
            for (int i = _env.Count - 1; i > -1; i--)
            {
                var frame = _env[i];
                if (frame.ZeroArityCallables.ContainsKey(name))
                {
                    return frame.ZeroArityCallables[name];
                }
            }
            throw Error($"Function {name}/0 is not known.", stack);
        }

        public Exception Error(string message, IMashStack stack)
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
            if (topFrame != null)
            {
                var position = SourceInformation?.GetProgramPosition(topFrame);
                if (position != null)
                {
                    line = programWithLines.GetLineNumber(position.StartPosition) + 1;
                    column = programWithLines.GetColumnNumber(position.StartPosition) + 1;
                }
            }
            return new JsonMasherException(message, line, column, stackSb.ToString());
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
