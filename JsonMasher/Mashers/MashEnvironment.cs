using System.Collections.Generic;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers
{
    public class MashEnvironment
    {
        Dictionary<string, Json> _variables { get; } = new();
        Dictionary<FunctionName, Callable> _callables { get; } = new();
        Dictionary<string, Callable> _zeroArityCallables { get; } = new();
        public static MashEnvironment DefaultEnvironment => _defaultEnvironment;

        MashEnvironment _previous;

        public MashEnvironment PushFrame()
        {
            MashEnvironment newOne = new();
            newOne._previous = this;
            return newOne;
        }

        public MashEnvironment PopFrame()
        {
            return _previous;
        }

        public void SetVariable(string name, Json value) => _variables[name] = value;

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

        public Json GetVariable(string name, IMashStack stack)
        {
            if (_variables.ContainsKey(name))
            {
                return _variables[name];
            }
            else if (_previous != null)
            {
                return _previous.GetVariable(name, stack);
            }
            else
            {
                return null;
            }
        }

        public void SetCallable(string name, Callable value) => _zeroArityCallables[name] = value;

        public Callable GetCallable(FunctionName name, IMashStack stack)
        {
            if (name.Arity == 0)
            {
                return GetCallableZero(name.Name, stack);
            }
            else
            {
                return GetCallableNonZero(name, stack);
            }
        }

        public Callable GetCallable(string name, IMashStack stack) 
            => GetCallableZero(name, stack);

        private static MashEnvironment _defaultEnvironment = InitDefaultEnvironment();

        private static MashEnvironment InitDefaultEnvironment()
        {
            var environment = new MashEnvironment();
            environment.SetCallable(new FunctionName("not", 0), Not.Builtin);
            environment.SetCallable(new FunctionName("empty", 0), Empty.Builtin);
            environment.SetCallable(new FunctionName("range", 1), Range.Builtin_1);
            environment.SetCallable(new FunctionName("range", 2), Range.Builtin_2);
            environment.SetCallable(new FunctionName("range", 3), Range.Builtin_3);
            environment.SetCallable(new FunctionName("length", 0), Length.Builtin);
            environment.SetCallable(new FunctionName("limit", 2), Limit.Builtin);
            return environment;
        }

        private Callable GetCallableZero(string name, IMashStack stack)
        {
            if (_zeroArityCallables.ContainsKey(name))
            {
                return _zeroArityCallables[name];
            }
            else if (_previous != null)
            {
                return _previous.GetCallableZero(name, stack);
            }
            else
            {
                return null;
            }
        }

        private Callable GetCallableNonZero(FunctionName name, IMashStack stack)
        {
            if (_callables.ContainsKey(name))
            {
                return _callables[name];
            }
            else if (_previous != null)
            {
                return _previous.GetCallableNonZero(name, stack);
            }
            else
            {
                return null;
            }
        }
    }
}
