using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators.LetMatchers
{
    public class ArrayMatcher : IMatcher
    {
        IMatcher[] _elements;
        public IReadOnlyCollection<IMatcher> Elements => _elements;

        public ArrayMatcher(params IMatcher[] elements) => _elements = elements;

        public IEnumerable<LetMatch> GetMatches(Json value, IMashContext context)
        {
            if (value == null || value.Type != JsonValueType.Array)
            {
                throw context.Error($"Expected an array, not {value?.Type}", value);
            }
            for (int i = 0; i < _elements.Length; i++)
            {
                var elementValue = i < value.GetLength() ? value.GetElementAt(i) : Json.Null;
                foreach (var match in _elements[i].GetMatches(elementValue, context))
                {
                    yield return match;
                }
            }
        }
    }
}
