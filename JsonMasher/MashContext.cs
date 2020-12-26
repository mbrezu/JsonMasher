using System.Collections.Generic;

namespace JsonMasher
{
    public class MashContext : IMashContext
    {
        List<Json> _log = new();
        List<Dictionary<string, Json>> _env = new();

        public IEnumerable<Json> Log => _log;

        public void LogValue(Json value)
        {
            _log.Add(value);
        }

        public void PushEnvironmentFrame()
        {
            _env.Add(new Dictionary<string, Json>());
        }

        public void PopEnvironmentFrame()
        {
            _env.RemoveAt(_env.Count - 1);
        }

        public void SetVariable(string name, Json value)
        {
            _env[_env.Count - 1][name] = value;
        }

        public Json GetVariable(string name)
        {
            return _env[_env.Count - 1][name];
        }

    }
}
