using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Primitives
{
    public class Enumerate : IJsonMasherOperator, IJsonZipper
    {
        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
            => json.Type switch 
            {
                JsonValueType.Array => json.EnumerateArray(),
                JsonValueType.Object => json.EnumerateObject().Select(kv => kv.Value),
                _ => throw new InvalidOperationException("Can't enumerate value.")
            };

        public ZipStage ZipDown(Json json, IMashContext context, IMashStack stack)
            => json.Type switch 
            {
                JsonValueType.Array => new ZipStage(
                    parts => Json.Array(parts),
                    Mash(json, context, stack)),
                JsonValueType.Object => ZipDownObject(json),
                _ => throw new InvalidOperationException("Can't enumerate value.")
            };

        private ZipStage ZipDownObject(Json json)
        {
            var properties = json.EnumerateObject();
            return new ZipStage(
                newValues => Json.Object(properties.Zip(newValues, (prop, val) => new JsonProperty(prop.Key, val))),
                properties.Select(kv => kv.Value)
            );
        }

        private Enumerate()
        {
        }

        private static Enumerate _instance = new Enumerate();
        public static Enumerate Instance => _instance;
    }
}
