using System;
using System.Collections.Generic;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers
{
    public class MashEnvironment
    {
        Dictionary<string, Json> _variables { get; } = new();
        Dictionary<FunctionName, Callable> _callables { get; } = new();
        Dictionary<string, Callable> _zeroArityCallables { get; } = new();

        MashEnvironment _previous;

        public MashEnvironment Push()
        {
            MashEnvironment newOne = new();
            newOne._previous = this;
            return newOne;
        }

        public MashEnvironment Pop()
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

        public Json GetVariable(string name, IMashStack stack, IMashContext context)
        {
            if (_variables.ContainsKey(name))
            {
                return _variables[name];
            }
            else if (_previous != null)
            {
                return _previous.GetVariable(name, stack, context);
            }
            else
            {
                throw context.Error($"Cannot find variable ${name}.", stack);
            }
            
        }

        public void SetCallable(string name, Callable value) => _zeroArityCallables[name] = value;

        public Callable GetCallable(FunctionName name, IMashStack stack, IMashContext context)
        {
            if (name.Arity == 0)
            {
                return GetCallableZero(name.Name, stack, context);
            }
            else
            {
                return GetCallableNonZero(name, stack, context);
            }
        }

        public Callable GetCallable(string name, IMashStack stack, IMashContext context) 
            => GetCallableZero(name, stack, context);

        private Callable GetCallableZero(string name, IMashStack stack, IMashContext context)
        {
            if (_zeroArityCallables.ContainsKey(name))
            {
                return _zeroArityCallables[name];
            }
            else if (_previous != null)
            {
                return _previous.GetCallableZero(name, stack, context);
            }
            else
            {
                throw context.Error($"Function {name}/0 is not known.", stack);
            }
        }

        private Callable GetCallableNonZero(FunctionName name, IMashStack stack, IMashContext context)
        {
            if (_callables.ContainsKey(name))
            {
                return _callables[name];
            }
            else if (_previous != null)
            {
                return _previous.GetCallableNonZero(name, stack, context);
            }
            else
            {
                throw context.Error($"Function {name.Name}/{name.Arity} is not known.", stack);
            }
        }

    }
}
