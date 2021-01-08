using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class StrIndices
    {
        private static Builtin _builtin
            = new Builtin(
                (mashers, haystack, context) => StrIndicesImpl(mashers, haystack, context),
                1);

        private static IEnumerable<Json> StrIndicesImpl(
            List<IJsonMasherOperator> mashers, Json haystack, IMashContext context)
        {
            if (haystack == null || haystack.Type != JsonValueType.String)
            {
                throw context.Error($"_strindices: cannot index over {haystack?.Type}.", haystack);
            }
            foreach (var needle in mashers[0].Mash(haystack, context))
            {
                if (needle == null || needle.Type != JsonValueType.String)
                {
                    throw context.Error($"_strindices: cannot index over {needle?.Type}.", needle);
                }
                var result = new List<Json>();
                int start = 0;
                while (start >= 0)
                {
                    start = haystack.GetString().IndexOf(needle.GetString(), start);
                    if (start >= 0)
                    {
                        result.Add(Json.Number(start));
                        start ++;
                    }
                }
                yield return Json.Array(result);
            }
        }

        public static Builtin Builtin => _builtin;
    }
}
