using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Combinators
{
    public class ConstructObject : IJsonMasherOperator
    {
        public record PropertyDescriptor(string Key, IJsonMasherOperator Operator);

        private PropertyDescriptor[] _propertyDescriptors;

        public ConstructObject(params PropertyDescriptor[] propertyDescriptors)
        {
            _propertyDescriptors = propertyDescriptors;
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => Mash(json, context, new List<JsonProperty>());

        private IEnumerable<Json> Mash(
            Json json, IMashContext context, List<JsonProperty> properties)
        {
            int level = properties.Count;
            if (level == _propertyDescriptors.Length)
            {
                yield return Json.Object(properties);
            }
            else
            {
                var descriptor = _propertyDescriptors[level];
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
