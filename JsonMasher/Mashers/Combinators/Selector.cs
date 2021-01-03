using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Combinators
{
    public class Selector : IJsonMasherOperator, IJsonZipper
    {
        public IJsonMasherOperator Index { get; init; }
        public bool IsOptional { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            context.Tick(stack);
            return MashOne(json, context, stack.Push(this));
        }

        private IEnumerable<Json> MashOne(Json json, IMashContext context, IMashStack stack)
        {
            foreach (var index in Index.Mash(json, context, stack))
            {
                var value = (index.Type, json.Type) switch {
                    (JsonValueType.Number, JsonValueType.Array) => json.GetElementAt((int)(index.GetNumber())),
                    (JsonValueType.String, JsonValueType.Object) => json.GetElementAt(index.GetString()),
                    _ => !IsOptional 
                        ? throw context.Error($"Can't index {json.Type} with {index.Type}.", stack, json, index)
                        : null
                };
                if (value != null)
                {
                    yield return value;
                }
            }
        }

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
            return new ZipStage(
                values => UpdateArray(json, intIndices, values.ToArray()),
                intIndices.Select(i => json.GetElementAt(i)));
        }

        private Json UpdateArray(Json json, int[] intIndices, Json[] values)
        {
            for (int i = 0; i < intIndices.Length; i++)
            {
                json = json.SetElementAt(intIndices[i], values[i]);
            }
            return json;
        }

        private ZipStage ZipDownObject(
            IEnumerable<Json> indices, Json json, IMashContext context, IMashStack stack)
        {
            if (!indices.All(x => x.Type == JsonValueType.String))
            {
                throw context.Error("Not all indices are strings.", stack, indices.ToArray());
            }
            var stringIndices = indices.Select(i => i.GetString()).ToArray();
            return new ZipStage(
                values => UpdateObject(json, stringIndices, values.ToArray()),
                stringIndices.Select(i => json.GetElementAt(i)));
        }

        private Json UpdateObject(Json json, string[] stringIndices, Json[] values)
        {
            for (int i = 0; i < stringIndices.Length; i++)
            {
                json = json.SetElementAt(stringIndices[i], values[i]);
            }
            return json;
        }
    }
}
