using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JsonMasher.JsonRepresentation
{
    class JsonArray : Json
    {
        ImmutableList<Json> _values;

        public JsonArray(IEnumerable<Json> values)
        {
            _values = ImmutableList<Json>.Empty;
            _values = _values.AddRange(values);
            Type = JsonValueType.Array;
        }

        private JsonArray(ImmutableList<Json> values)
        {
            _values = values;
            Type = JsonValueType.Array;
        }

        public override IEnumerable<Json> EnumerateArray() => _values;

        public override Json GetElementAt(int index)
        {
            index = AdjustIndex(index);
            if (index < 0 || index >= _values.Count)
            {
                return Null;
            }
            else
            {
                return _values[index];
            }
        }

        public override bool ContainsKey(int index)
        {
            index = AdjustIndex(index);
            return index >= 0 && index < _values.Count;
        }

        public override int GetLength() => _values.Count;

        public override Json SetElementAt(int index, Json value)
        {
            index = AdjustIndex(index);
            return new JsonArray(_values.SetItem(index, value));
        }

        public override Json DelElementAt(int index)
        {
            index = AdjustIndex(index);
            return new JsonArray(_values.RemoveAt(index));
        }

        private int AdjustIndex(int index) => index >= 0 ? index : _values.Count + index;

        public override Json GetSliceAt(int start, int end)
        {
            var slice = new List<Json>();
            for (int i = start; i < end; i++)
            {
                slice.Add(GetElementAt(i));
            }
            return Json.Array(slice);
        }

        public override Json SetSliceAt(int start, int end, Json value)
            => Json.Array(
                EnumerateArray().Take(start)
                    .Concat(value.EnumerateArray())
                    .Concat(EnumerateArray().Skip(end)));

        public override Json DelSliceAt(int start, int end)
            => Json.Array(EnumerateArray().Take(start).Concat(EnumerateArray().Skip(end)));
    }
}
