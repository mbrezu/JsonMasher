using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers
{
    public class VariablesEnvironment
    {
        Dictionary<string, Json> _variables { get; } = new();
        VariablesEnvironment _previous;

        public VariablesEnvironment PushFrame()
        {
            VariablesEnvironment newOne = new();
            newOne._previous = this;
            return newOne;
        }

        public void SetVariable(string name, Json value)
        {
            _variables[name] = value;
        }

        private int Count()
        {
            if (_previous == null)
            {
                return 1;
            }
            else
            {
                return 1 + _previous.Count();
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
    }
}
