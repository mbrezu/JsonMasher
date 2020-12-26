using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Tests
{
    public static class Utils
    {
        public static (IEnumerable<Json> json, IMashContext context) RunAsSequenceWithContext(
            this IJsonMasherOperator op, Json data)
        {
            var masher = new JsonMasher();
            var result = masher.Mash(data.AsEnumerable(), op);
            return (json: result, context: masher.Context);
        }

        public static IEnumerable<Json> RunAsSequence(this IJsonMasherOperator op, Json data)
            => op.RunAsSequenceWithContext(data).json;

        public static Json RunAsScalar(this IJsonMasherOperator op, Json data)
            => op.RunAsSequence(data).First();

        public static (Json json, IMashContext context) RunAsScalarWithContext(
            this IJsonMasherOperator op, Json data)
        {
            var (json, context) = op.RunAsSequenceWithContext(data);
            return (json.First(), context);
        }
    }
}
