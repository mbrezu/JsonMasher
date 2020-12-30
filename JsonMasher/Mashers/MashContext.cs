using System;
using System.Collections.Generic;
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

        public IEnumerable<Json> Log => _log;

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

        public Json GetVariable(string name)
        {
            for (int i = _env.Count - 1; i > -1; i--)
            {
                var frame = _env[i];
                if (frame.Variables.ContainsKey(name))
                {
                    return frame.Variables[name];
                }
            }
            throw new InvalidOperationException();
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

        public Callable GetCallable(FunctionName name)
        {
            if (name.Arity == 0)
            {
                return GetCallable(name.Name);
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
                throw new InvalidOperationException();
            }
        }

        public void SetCallable(string name, Callable value)
        {
            _env[_env.Count - 1].ZeroArityCallables[name] = value;
        }

        public Callable GetCallable(string name)
        {
            for (int i = _env.Count - 1; i > -1; i--)
            {
                var frame = _env[i];
                if (frame.ZeroArityCallables.ContainsKey(name))
                {
                    return frame.ZeroArityCallables[name];
                }
            }
            throw new InvalidOperationException();
        }
    }
}
