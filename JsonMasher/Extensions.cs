using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace JsonMasher
{
    public static class Extensions
    {
        public static Json AsIJson(this JsonDocument doc)
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
    }
}