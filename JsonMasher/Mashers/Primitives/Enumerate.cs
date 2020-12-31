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
                _ => throw context.Error($"Can't enumerate {json.Type}.", stack.Push(this), json)
            };

        public ZipStage ZipDown(Json json, IMashContext context, IMashStack stack)
            => json.Type switch 
            {
                JsonValueType.Array => new ZipStage(
                    parts => Json.Array(parts),
                    Mash(json, context, stack)),
                JsonValueType.Object => ZipDownObject(json),
                _ => throw context.Error($"Can't enumerate {json.Type}.", stack.Push(this), json)
            };

        private ZipStage ZipDownObject(Json json)
        {
            var properties = json.EnumerateObject();
            return new ZipStage(
                newValues => Json.Object(properties.Zip(newValues, (prop, val) => new JsonProperty(prop.Key, val))),
                properties.Select(kv => kv.Value)
            );
        }

        // All these are the same for testing/FluentAssertions purposes,
        // but different for SourceInformation purposes, so the singletons were removed.
        public override bool Equals(object obj) => obj is Enumerate;
        public override int GetHashCode() => 1;
    }
}
