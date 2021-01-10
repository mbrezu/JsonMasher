using System.Collections.Generic;
using System.Text;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Implode
    {
        private static Builtin _builtin = new Builtin(Function, 0);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            if (json == null || json.Type != JsonValueType.Array)
            {
                throw context.Error($"Need an array of numbers to implode, not {json?.Type}.", json);
            }
            var sb = new StringBuilder();
            foreach (var value in json.EnumerateArray())
            {
                if (value == null || value.Type != JsonValueType.Number)
                {
                    throw context.Error($"Needed a number, but got {value?.Type}.", value);
                }
                sb.Append(((char)(int)value.GetNumber()).ToString());
            }
            yield return Json.String(sb.ToString());
        }

        public static Builtin Builtin => _builtin;
    }
}
