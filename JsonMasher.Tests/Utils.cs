using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Tests
{
    public static class Utils
    {
        public static Json RunAsScalar(this IJsonMasherOperator op, Json data)
            => op.RunAsSequence(data).First();

        public static IEnumerable<Json> RunAsSequence(this IJsonMasherOperator op, Json data)
            => new JsonMasher().Mash(data.AsEnumerable(), op);

    }
}
