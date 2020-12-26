using System.Collections.Generic;
using JsonMasher.Combinators;

namespace JsonMasher
{
    public interface IMashContext
    {
        void LogValue(Json value);
        IEnumerable<Json> Log { get; }
        void PushEnvironmentFrame();
        void PopEnvironmentFrame();
        void SetVariable(string name, Json value);
        Json GetVariable(string name);
        void SetCallable(string name, Callable value);
        Callable GetCallable(string name);
    }
}
