using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using JsonMasher.Mashers;
using JsonMasher.Mashers.Primitives;

namespace JsonMasher
{
    public static class Extensions
    {
        public static Json AsJson(this JsonDocument doc)
            => doc.RootElement.AsJson();

        public static Json AsJson(this JsonElement element)
            => element.ValueKind switch
            {
                JsonValueKind.Undefined => Json.Undefined,
                JsonValueKind.Null => Json.Null,
                JsonValueKind.True => Json.True,
                JsonValueKind.False => Json.False,
                JsonValueKind.Number => Json.Number(element.GetDouble()),
                JsonValueKind.String => Json.String(element.GetString()),
                JsonValueKind.Array => new JsonArray(
                    element.EnumerateArray().Select(elt => elt.AsJson())),
                JsonValueKind.Object => new JsonObject(
                    element
                    .EnumerateObject()
                    .Select(jp => new JsonProperty(jp.Name, jp.Value.AsJson()))),
                _ => throw new InvalidOperationException()
            };

        public static IEnumerable<Json> AsEnumerable(this Json json)
        {
            yield return json;
        }

        internal static IJsonMasherOperator Fold(
            this IEnumerable<IJsonMasherOperator> mashers,
            Func<IJsonMasherOperator, IJsonMasherOperator, IJsonMasherOperator> combiner)
        => mashers.Count() switch
            {
                0 => Identity.Instance,
                1 => mashers.First(),
                _ => combiner(mashers.First(), mashers.Skip(1).Fold(combiner))
            };

        public static string Repeat(this string str, int times)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < times; i++)
            {
                sb.Append(str);
            }
            return sb.ToString();
        }
    }
}