using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers;

namespace JsonMasher.Tests
{
    public static class Utils
    {
        public static (IEnumerable<Json> json, IMashContext context) RunAsSequenceWithContext(
            this IJsonMasherOperator op, Json data)
        {
            var masher = new Mashers.JsonMasher();
            var result = masher.Mash(data.AsEnumerable(), op, DefaultMashStack.Instance);
            return (json: result.sequence, context: result.context);
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

        public static Json JsonNumberArray(params double[] values)
            => Json.Array(values.Select(x => Json.Number(x)));

        public static string CleanCR(this string str) => str.Replace("\r", "");
    }
}
