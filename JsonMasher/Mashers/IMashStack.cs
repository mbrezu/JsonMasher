using System;
using System.Collections.Generic;

namespace JsonMasher.Mashers
{
    public interface IMashStack
    {
        IMashStack Push(IJsonMasherOperator op);
        IEnumerable<IJsonMasherOperator> GetValues();
        IJsonMasherOperator Top { get; }
    }
}
