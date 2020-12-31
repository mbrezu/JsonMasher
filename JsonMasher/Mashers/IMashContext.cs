using System;
using System.Collections.Generic;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers
{
    public interface IMashContext
    {
        void LogValue(Json value);
        IEnumerable<Json> Log { get; }
        void PushEnvironmentFrame();
        void PopEnvironmentFrame();
        void SetVariable(string name, Json value);
        Json GetVariable(string name);
        void SetCallable(FunctionName name, Callable value);
        Callable GetCallable(FunctionName name);
        void SetCallable(string name, Callable value);
        Callable GetCallable(string name);
        Exception Error(string message, IMashStack stack);
        void Tick(IMashStack stack);
    }
}
