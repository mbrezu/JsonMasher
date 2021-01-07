using System.Collections.Generic;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers
{
    public class CallablesEnvironment
    {
        Dictionary<FunctionName, Callable> _callables { get; } = new();
        Dictionary<string, Callable> _zeroArityCallables { get; } = new();
        CallablesEnvironment _previous;

        public CallablesEnvironment PushFrame()
        {
            CallablesEnvironment newOne = new();
            newOne._previous = this;
            return newOne;
        }

        public void SetCallable(FunctionName name, Callable value)
        {
            if (name.Arity == 0)
            {
                SetCallable(name.Name, value);
            }
            else
            {
                _callables[name] = value;
            }
        }

        public void SetCallable(string name, Callable value) => _zeroArityCallables[name] = value;

        public void SetCallable(string name, List<string> arguments, IJsonMasherOperator body)
        {
            if (arguments == null || arguments.Count == 0)
            {
                SetCallable(name, body);
            }
            else
            {
                SetCallable(
                    new FunctionName(name, arguments.Count),
                    new Function(body, arguments));
            }
        }

        public Callable GetCallable(FunctionName name)
        {
            if (name.Arity == 0)
            {
                return GetCallableZero(name.Name);
            }
            else
            {
                return GetCallableNonZero(name);
            }
        }

        public Callable GetCallable(string name) => GetCallableZero(name);

        private Callable GetCallableZero(string name)
        {
            if (_zeroArityCallables.ContainsKey(name))
            {
                return _zeroArityCallables[name];
            }
            else if (_previous != null)
            {
                return _previous.GetCallableZero(name);
            }
            else
            {
                return null;
            }
        }

        private Callable GetCallableNonZero(FunctionName name)
        {
            if (_callables.ContainsKey(name))
            {
                return _callables[name];
            }
            else if (_previous != null)
            {
                return _previous.GetCallableNonZero(name);
            }
            else
            {
                return null;
            }
        }
    }
}
