using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public record PropertyDescriptor(IJsonMasherOperator Key, IJsonMasherOperator Value);

    public class ConstructObject : IJsonMasherOperator
    {
        public IReadOnlyList<PropertyDescriptor> Descriptors { get; init; }

        public ConstructObject(params PropertyDescriptor[] descriptors)
        {
            Descriptors = descriptors.ToList();
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => Mash(json, context.PushStack(this), new List<JsonProperty>());

        private IEnumerable<Json> Mash(
            Json json, IMashContext context, List<JsonProperty> properties)
        {
            int level = properties.Count;
            if (level == Descriptors.Count)
            {
                context.Tick();
                yield return Json.Object(properties);
            }
            else
            {
                var descriptor = Descriptors[level];
                foreach (var key in descriptor.Key.Mash(json, context))
                {
                    if (key.Type != JsonValueType.String)
                    {
                        context.Error(
                            $"While building an object, cannot index object with {key.Type}", key);
                    }
                    foreach (var value in descriptor.Value.Mash(json, context))
                    {
                        properties.Add(new JsonProperty(key.GetString(), value));
                        foreach (var result in Mash(json, context, properties))
                        {
                            yield return result;
                        }
                        properties.RemoveAt(level);
                    }
                }
            }
        }
    }
}
