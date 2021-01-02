using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Combinators
{
    public class Selector : IJsonMasherOperator, IJsonZipper
    {
        public IJsonMasherOperator Index { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            context.Tick(stack);
            return MashOne(json, context, stack.Push(this));
        }

        private IEnumerable<Json> MashOne(Json json, IMashContext context, IMashStack stack)
            => Index.Mash(json, context, stack).Select(index => 
                (index.Type, json.Type) switch {
                    (JsonValueType.Number, JsonValueType.Array) => json.GetElementAt((int)(index.GetNumber())),
                    (JsonValueType.String, JsonValueType.Object) => json.GetElementAt(index.GetString()),
                    _ => throw context.Error($"Can't index {json.Type} with {index.Type}.", stack, json, index)
                });

        public ZipStage ZipDown(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(newStack);
            var indices = Index.Mash(json, context, newStack);
            return json.Type switch {
                JsonValueType.Array => ZipDownArray(indices, json, context, newStack),
                JsonValueType.Object => ZipDownObject(indices, json, context, newStack),
                _ => throw context.Error($"Can't iterate {json.Type}.", newStack, json)
            };
        }

        private ZipStage ZipDownArray(
            IEnumerable<Json> indices, Json json, IMashContext context, IMashStack stack)
        {
            if (!indices.All(x => x.Type == JsonValueType.Number))
            {
                throw context.Error("Not all indices are numbers.", stack, indices.ToArray());
            }
            var intIndices = indices.Select(i => (int)(i.GetNumber())).ToArray();
            var parts = intIndices.Select(i => json.GetElementAt(i));
            return new ZipStage(
                values => RecombineArray(json, intIndices, values.ToArray()),
                parts);
        }

        private Json RecombineArray(Json json, int[] intIndices, Json[] values)
        {
            var jsonValues = json.EnumerateArray().ToArray();
            for (int i = 0; i < intIndices.Length; i++)
            {
                jsonValues[intIndices[i]] = values[i];
            }
            return Json.Array(jsonValues);
        }

        private ZipStage ZipDownObject(
            IEnumerable<Json> indices, Json json, IMashContext context, IMashStack stack)
        {
            if (!indices.All(x => x.Type == JsonValueType.String))
            {
                throw context.Error("Not all indices are strings.", stack, indices.ToArray());
            }
            var stringIndices = indices.Select(i => i.GetString()).ToArray();
            var parts = stringIndices.Select(i => json.GetElementAt(i));
            return new ZipStage(
                values => RecombineObject(json, stringIndices, values.ToArray()),
                parts);
        }

        private Json RecombineObject(Json json, string[] stringIndices, Json[] values)
            => Json.Object(
                json.EnumerateObject()
                .Concat(stringIndices.Zip(values, (i, v) => new JsonProperty(i, v))));
    }
}
