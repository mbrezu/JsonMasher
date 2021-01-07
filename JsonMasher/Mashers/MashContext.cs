using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers
{
    public class ContextMutableState
    {
        public List<Json> Log { get; init; } = new();
        public int Ticks { get; set; }
        public int TickLimit { get; init; }

        public void Tick()
        {
            if (TickLimit != 0)
            {
                Ticks++;
            }
        }

        public bool OverTickLimit() => TickLimit != 0 && Ticks > TickLimit;
    }

    public record MashContext(ContextMutableState MutableState,
        VariablesEnvironment EnvVariables,
        CallablesEnvironment EnvCallables,
        SourceInformation SourceInformation) : IMashContext
    {
        public IEnumerable<Json> Log => MutableState.Log;

        public static MashContext CreateContext(
            int tickLimit = 0, SourceInformation sourceInformation = null)
            => new MashContext(
                new() { TickLimit = tickLimit },
                new(),
                StandardLibrary.DefaultEnvironment.PushFrame(),
                sourceInformation);

        public void LogValue(Json value) => MutableState.Log.Add(value);

        public IMashContext PushVariablesFrame()
            => this with { EnvVariables = EnvVariables.PushFrame() };

        public IMashContext PushCallablesFrame()
            => this with { EnvCallables = EnvCallables.PushFrame() };

        public void SetVariable(string name, Json value) => EnvVariables.SetVariable(name, value);

        public Json GetVariable(string name, IMashStack stack)
            => EnvVariables.GetVariable(name, stack)
                ?? throw Error($"Cannot find variable ${name}.", stack);

        public void SetCallable(FunctionName name, Callable value)
            => EnvCallables.SetCallable(name, value);

        public Callable GetCallable(FunctionName name, IMashStack stack)
        {
            var result = EnvCallables.GetCallable(name, stack)
                ?? throw Error($"Function {name.Name}/{name.Arity} is not known.", stack);
            return result;
        }

        public void SetCallable(string name, List<string> arguments, IJsonMasherOperator body)
            => EnvCallables.SetCallable(name, arguments, body);

        public void SetCallable(string name, Callable value) => EnvCallables.SetCallable(name, value);

        public Callable GetCallable(string name, IMashStack stack)
            => EnvCallables.GetCallable(name, stack)
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
            MutableState.Tick();
            if (MutableState.OverTickLimit())
            {
                throw Error($"Failed to complete in {MutableState.TickLimit} ticks.", stack);
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
