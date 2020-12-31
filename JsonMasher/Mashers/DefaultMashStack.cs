using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers
{
    public class DefaultMashStack : IMashStack
    {
        public IEnumerable<IJsonMasherOperator> GetValues() 
            => Enumerable.Empty<IJsonMasherOperator>();

        public IMashStack Push(IJsonMasherOperator op) => this;

        private DefaultMashStack() { }
        public static DefaultMashStack Instance { get; } = new DefaultMashStack();
        public IJsonMasherOperator Top => null;
    }
}
