using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Primitives
{
    public class StringSelector : IJsonMasherOperator, IJsonZipper
    {
        public string Key { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
            => MashOne(json, context, stack.Push(this)).AsEnumerable();

        public ZipStage ZipDown(Json json, IMashContext context, IMashStack stack)
        {
            context.Tick(stack);
            if (json.Type == JsonValueType.Object)
            {
                return new ZipStage(
                    val => Json.Object(
                        json.EnumerateObject()
                            .Concat(new [] { new JsonProperty(Key, val.First())})),
                    json.GetElementAt(Key).AsEnumerable());
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private Json MashOne(Json json, IMashContext context, IMashStack stack)
            => json.Type switch {
                JsonValueType.Object => json.GetElementAt(Key),
                _ => throw context.Error($"Can't index {json.Type} with a string.", stack, json)
            };
    }
}
