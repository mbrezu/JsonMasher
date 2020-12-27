using System.Collections.Generic;

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
        void SetCallable(string name, Callable value);
        Callable GetCallable(string name);
    }
}
