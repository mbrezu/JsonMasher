using System.Collections.Generic;
using System.Collections.Immutable;

namespace JsonMasher.Mashers
{
    public class DebugMashStack : IMashStack
    {
        private ImmutableStack<IJsonMasherOperator> _stack;

        public IJsonMasherOperator Top => _stack.IsEmpty ? null : _stack.Peek();

        public DebugMashStack() => _stack = ImmutableStack<IJsonMasherOperator>.Empty;
        
        public IEnumerable<IJsonMasherOperator> GetValues() => _stack;

        public IMashStack Push(IJsonMasherOperator op) => new DebugMashStack(_stack.Push(op));

        private DebugMashStack(ImmutableStack<IJsonMasherOperator> stack) => _stack = stack;
    }
}
