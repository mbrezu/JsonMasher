using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JsonMasher.Compiler;

namespace JsonMasher.JsonRepresentation
{
    class JsonObject : Json
    {
        ImmutableDictionary<string, Json> _values;

        public JsonObject(IEnumerable<JsonProperty> values)
        {
            _values = ImmutableDictionary<string, Json>.Empty;
            _values = _values.SetItems(values.Select(kv => new KeyValuePair<string, Json>(kv.Key, kv.Value)));
            Type = JsonValueType.Object;
        }

        private JsonObject(ImmutableDictionary<string, Json> values)
        {
            Type = JsonValueType.Object;
            _values = values;
        }

        public override IEnumerable<JsonProperty> EnumerateObject()
        {
            foreach (var kv in _values.OrderBy(kv => kv.Key))
            {
                yield return new JsonProperty(kv.Key, kv.Value);
            }
        }

        public override Json GetElementAt(string key)
        {
            Json result;
            if (_values.TryGetValue(key, out result))
            {
                return result;
            }
            else
            {
                return Null;
            }
        }

        public override bool ContainsKey(string key) => _values.ContainsKey(key);

        public override int GetLength() => _values.Count;

        public override Json SetElementAt(string key, Json value)
            => new JsonObject(_values.SetItem(key, value));

        public override Json DelElementAt(string index) => new JsonObject(_values.Remove(index));

        public override JsonPathPart GetPathPart()
        {
            var start = GetElementAt("start");
            if (start == null)
            {
                throw new JsonMasherException("Can't find a 'start' key.", null, this);
            }
            if (start.Type != JsonValueType.Number)
            {
                throw new JsonMasherException("Value for 'start' is not a number.", null, this);
            }
            var end = GetElementAt("end");
            if (end == null)
            {
                throw new JsonMasherException("Can't find a 'end' key.", null, this);
            }
            if (end.Type != JsonValueType.Number)
            {
                throw new JsonMasherException("Value for 'end' is not a number.", null, this);
            }
            return new SlicePathPart((int)start.GetNumber(), (int)end.GetNumber());
        }
    }
}
