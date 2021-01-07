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

    public record ContextState(
        ContextMutableState MutableState,
        VariablesEnvironment EnvVariables,
        CallablesEnvironment EnvCallables,
        SourceInformation SourceInformation);

    public class MashContext : IMashContext
    {
        private ContextState _state;
        public IEnumerable<Json> Log => _state.MutableState.Log;

        public MashContext(int tickLimit = 0, SourceInformation sourceInformation = null)
        {
            _state = new ContextState(
                new() { TickLimit = tickLimit },
                new(),
                StandardLibrary.DefaultEnvironment.PushFrame(),
                sourceInformation);
        }

        private MashContext(ContextState state)
        {
            _state = state;
        }

        public void LogValue(Json value) => _state.MutableState.Log.Add(value);

        public IMashContext PushVariablesFrame()
        {
            return new MashContext(_state with { EnvVariables = _state.EnvVariables.PushFrame() });
        }

        public IMashContext PushCallablesFrame()
        {
            return new MashContext(_state with { EnvCallables = _state.EnvCallables.PushFrame() });
        }

        public void SetVariable(string name, Json value) => _state.EnvVariables.SetVariable(name, value);

        public Json GetVariable(string name, IMashStack stack)
            => _state.EnvVariables.GetVariable(name, stack)
                ?? throw Error($"Cannot find variable ${name}.", stack);

        public void SetCallable(FunctionName name, Callable value) => _state.EnvCallables.SetCallable(name, value);

        public Callable GetCallable(FunctionName name, IMashStack stack)
        {
            var result = _state.EnvCallables.GetCallable(name, stack)
                ?? throw Error($"Function {name.Name}/{name.Arity} is not known.", stack);
            return result;
        }

        public void SetCallable(string name, List<string> arguments, IJsonMasherOperator body)
            => _state.EnvCallables.SetCallable(name, arguments, body);

        public void SetCallable(string name, Callable value) => _state.EnvCallables.SetCallable(name, value);

        public Callable GetCallable(string name, IMashStack stack)
            => _state.EnvCallables.GetCallable(name, stack)
                ?? throw Error($"Function {name}/0 is not known.", stack);

        public Exception Error(string message, IMashStack stack, params Json[] values)
        {
            var programWithLines = _state.SourceInformation != null
                ? new ProgramWithLines(_state.SourceInformation.Program)
                : null;
            var stackSb = new StringBuilder();
            foreach (var frame in stack.GetValues())
            {
                var position = _state.SourceInformation?.GetProgramPosition(frame);
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
                var position = _state.SourceInformation?.GetProgramPosition(topFrame);
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
            _state.MutableState.Tick();
            if (_state.MutableState.OverTickLimit())
            {
                throw Error($"Failed to complete in {_state.MutableState.TickLimit} ticks.", stack);
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
