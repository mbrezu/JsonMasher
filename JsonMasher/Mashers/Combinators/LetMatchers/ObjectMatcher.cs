using System.Collections.Generic;
using System.Linq;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators.LetMatchers
{
    public record ObjectMatcherProperty(IJsonMasherOperator Operator, IMatcher Matcher);

    public class ObjectMatcher : IMatcher
    {

        private ObjectMatcherProperty[] _properties;

        public ObjectMatcher(params ObjectMatcherProperty[] properties) => _properties = properties;

        public IEnumerable<LetMatch> GetMatches(Json value, IMashContext context)
        {
            if (value == null || value.Type != JsonValueType.Object)
            {
                throw context.Error($"Expected an object, not {value?.Type}", value);
            }
            foreach (var property in _properties)
            {
                var key = property.Operator.Mash(value, context).First(); // TODO: this needs to loop over all possible values
                if (key.Type != JsonValueType.String)
                {
                    throw context.Error($"Expected a string, not {key?.Type}", key);
                }
                var propertyValue = value.ContainsKey(key.GetString())
                    ? value.GetElementAt(key.GetString())
                    : Json.Null;
                foreach (var match in property.Matcher.GetMatches(propertyValue, context))
                {
                    yield return match;
                }
            }
        }
    }
}
