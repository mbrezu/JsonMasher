using System.Collections.Generic;

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
    }
}
