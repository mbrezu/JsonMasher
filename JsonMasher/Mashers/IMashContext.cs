using System;
using System.Collections.Generic;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers
{
    public interface IMashContext
    {
        void LogValue(Json value);
        IEnumerable<Json> Log { get; }
        IMashContext PushVariablesFrame();
        IMashContext PushCallablesFrame();
        IMashContext PushStack(IJsonMasherOperator masher);
        void SetVariable(string name, Json value);
        Json GetVariable(string name);
        void SetCallable(FunctionName name, Callable value);
        void SetCallable(string name, List<string> arguments, IJsonMasherOperator body);
        Callable GetCallable(FunctionName name);
        void SetCallable(string name, Callable value);
        Callable GetCallable(string name);
        Exception Error(string message, params Json[] values);
        void Tick();
        JsonPath GetPathFromArray(Json pathAsJson);
    }
}
