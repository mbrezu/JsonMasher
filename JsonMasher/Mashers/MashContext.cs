using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers
{
    public class MashContext : IMashContext
    {
        List<Json> _log = new();
        VariablesEnvironment _envVariables = new();
        CallablesEnvironment _envCallables = new();
        int ticks = 0;
        public int TickLimit { get; init; }
        public IEnumerable<Json> Log => _log;
        public SourceInformation SourceInformation { get; init; }

        public MashContext()
        {
            _envCallables = StandardLibrary.DefaultEnvironment;
            PushVariablesFrame();
            PushCallablesFrame();
        }

        public void LogValue(Json value) => _log.Add(value);

        public void PushVariablesFrame() => _envVariables = _envVariables.PushFrame();

        public void PopVariablesFrame() => _envVariables = _envVariables.PopFrame();

        public void PushCallablesFrame() => _envCallables = _envCallables.PushFrame();

        public void PopCallablesFrame() => _envCallables = _envCallables.PopFrame();

        public void SetVariable(string name, Json value) => _envVariables.SetVariable(name, value);

        public Json GetVariable(string name, IMashStack stack)
            => _envVariables.GetVariable(name, stack)
                ?? throw Error($"Cannot find variable ${name}.", stack);

        public void SetCallable(FunctionName name, Callable value) => _envCallables.SetCallable(name, value);

        public Callable GetCallable(FunctionName name, IMashStack stack)
        {
            var result = _envCallables.GetCallable(name, stack)
                ?? throw Error($"Function {name.Name}/{name.Arity} is not known.", stack);
            return result;
        }

        public void SetCallable(string name, List<string> arguments, IJsonMasherOperator body)
            => _envCallables.SetCallable(name, arguments, body);

        public void SetCallable(string name, Callable value) => _envCallables.SetCallable(name, value);

        public Callable GetCallable(string name, IMashStack stack)
            => _envCallables.GetCallable(name, stack)
                ?? throw Error($"Function {name}/0 is not known.", stack);

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
                ticks++;
                if (ticks > TickLimit)
                {
                    throw Error($"Failed to complete in {TickLimit} ticks.", stack);
                }
            }
        }

        public JsonPath GetPathFromArray(Json pathAsJson, IMashStack stack)
        {
            if (pathAsJson.Type != JsonValueType.Array)
            {
                throw Error($"Can't use {pathAsJson.Type} as a path.", stack, pathAsJson);
            }
            try
            {
                return JsonPath.FromParts(pathAsJson.EnumerateArray().Select(x => x.GetPathPart()));
            }
            catch (JsonMasherException ex)
            {
                throw Error(ex.Message, stack, ex.Values.ToArray());
            }
        }
    }
}
