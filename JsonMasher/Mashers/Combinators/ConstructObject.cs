using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Combinators
{
    public record PropertyDescriptor(string Key, IJsonMasherOperator Operator);

    public class ConstructObject : IJsonMasherOperator
    {
        public IReadOnlyList<PropertyDescriptor> Descriptors { get; init; }

        public ConstructObject(params PropertyDescriptor[] descriptors)
        {
            Descriptors = descriptors.ToList();
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => Mash(json, context, new List<JsonProperty>());

        private IEnumerable<Json> Mash(
            Json json, IMashContext context, List<JsonProperty> properties)
        {
            int level = properties.Count;
            if (level == Descriptors.Count)
            {
                yield return Json.Object(properties);
            }
            else
            {
                var descriptor = Descriptors[level];
                foreach (var value in descriptor.Operator.Mash(json, context))
                {
                    properties.Add(new JsonProperty(descriptor.Key, value));
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
