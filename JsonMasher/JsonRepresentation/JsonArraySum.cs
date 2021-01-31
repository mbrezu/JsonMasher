using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace JsonMasher.JsonRepresentation
{
    class JsonArraySum : JsonArray
    {
        ImmutableList<JsonArray> _arrays = ImmutableList<JsonArray>.Empty;
        private bool _convertedToArray;

        private JsonArraySum(ImmutableList<JsonArray> arrays)
        {
            _arrays = arrays;
            Type = JsonValueType.Array;
        }

        private JsonArraySum(params JsonArray[] arrays)
        {
            _arrays = ImmutableList<JsonArray>.Empty.AddRange(arrays);
            Type = JsonValueType.Array;
        }

        public Json AddOne(JsonArray one) => new JsonArraySum(_arrays.Add(one));

        public Json InsertOne(int index, JsonArray one)
            => new JsonArraySum(_arrays.Insert(index, one));

        public Json AddMany(JsonArraySum many)
            => new JsonArraySum(_arrays.AddRange(many._arrays));

        public override IEnumerable<Json> EnumerateArray()
        {
            ConvertToArray();
            return base.EnumerateArray();
        }

        public override Json GetElementAt(int index)
        {
            ConvertToArray();
            return base.GetElementAt(index);
        }

        public override bool ContainsKey(int index)
        {
            ConvertToArray();
            return base.ContainsKey(index);
        }

        public override int GetLength()
        {
            ConvertToArray();
            return base.GetLength();
        }

        public override Json SetElementAt(int index, Json value)
        {
            ConvertToArray();
            return base.SetElementAt(index, value);
        }

        public override Json DelElementAt(int index)
        {
            ConvertToArray();
            return base.DelElementAt(index);
        }

        public override Json GetSliceAt(int start, int end)
        {
            ConvertToArray();
            return base.GetSliceAt(start, end);
        }

        public override Json SetSliceAt(int start, int end, Json value)
        {
            ConvertToArray();
            return base.SetSliceAt(start, end, value);
        }

        public override Json DelSliceAt(int start, int end)
        {
            ConvertToArray();
            return base.DelSliceAt(start, end);
        }

        public static Json Plus(Json j1, Json j2)
        {
            if (j1 is JsonArraySum jas1 && !jas1._convertedToArray)
            {
                return AddToSum(jas1, j2);
            }
            else if (j1 is JsonArray ja1)
            {
                return AddToArray(ja1, j2);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static Json AddToSum(JsonArraySum jas1, Json j2)
        {
            if (j2 is JsonArraySum jas2 && !jas2._convertedToArray)
            {
                return jas1.AddMany(jas2);
            }
            else if (j2 is JsonArray ja2)
            {
                return jas1.AddOne(ja2);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static Json AddToArray(JsonArray ja1, Json j2)
        {
            if (j2 is JsonArraySum jas2 && !jas2._convertedToArray)
            {
                return jas2.InsertOne(0, ja1);
            }
            else if (j2 is JsonArray ja2)
            {
                return new JsonArraySum(ja1, ja2);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private void ConvertToArray()
        {
            if (_convertedToArray) {
                return;
            }
            var elements = new List<Json>();
            foreach (var array in _arrays)
            {
                elements.AddRange(array.EnumerateArray());
            }
            _values = _values.AddRange(elements);
            _arrays = ImmutableList<JsonArray>.Empty;
            _convertedToArray = true;
        }
    }
}
