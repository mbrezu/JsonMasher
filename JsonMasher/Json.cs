using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JsonMasher
{
    public record JsonProperty(string Key, Json Value);
    // Order is important, as it is the primary sort key
    // (see https://stedolan.github.io/jq/manual/#Builtinoperatorsandfunctions,
    // search for 'sort, sort_by').
    public enum JsonValueType : byte
    {
        Undefined,
        Null,
        False,
        True,
        Number,
        String,
        Array,
        Object,
    }

    public class JsonComparer : Comparer<Json>
    {
        public override int Compare(Json x, Json y)
        {
            if (x.Type < y.Type)
            {
                return -1;
            }
            else if (x.Type > y.Type)
            {
                return 1;
            }
            else
            {
                return x.Type switch {
                    JsonValueType.True 
                        or JsonValueType.False 
                        or JsonValueType.Null 
                        or JsonValueType.Undefined => 0,
                    JsonValueType.Number => x.GetNumber().CompareTo(y.GetNumber()),
                    JsonValueType.String => x.GetString().CompareTo(y.GetString()),
                    JsonValueType.Array => CompareArrays(x, y),
                    JsonValueType.Object => CompareObjects(x, y),
                    _ => throw new NotImplementedException()
                };
            }
        }

        private int CompareArrays(Json x, Json y)
        {
            int i = 0;
            while (true)
            {
                bool xAtEnd = i == x.GetLength();
                bool yAtEnd = i == y.GetLength();
                if (xAtEnd && yAtEnd)
                {
                    return 0;
                }
                if (xAtEnd && !yAtEnd)
                {
                    return -1;
                }
                if (!xAtEnd && yAtEnd)
                {
                    return 1;
                }
                var elementCompare = Compare(x.GetElementAt(i), y.GetElementAt(i));
                if (elementCompare != 0)
                {
                    return elementCompare;
                }
                i++;
            }
        }

        private int CompareObjects(Json x, Json y)
        {
            var xkv = x.EnumerateObject();
            var xk = xkv.Select(kv => Json.String(kv.Key));

            var ykv = y.EnumerateObject();
            var yk = ykv.Select(kv => Json.String(kv.Key));

            int keysCompare = Compare(Json.Array(xk), Json.Array(yk));
            if (keysCompare != 0)
            {
                return keysCompare;
            }

            var xv = xkv.Select(kv => kv.Value);
            var yv = ykv.Select(kv => kv.Value);
            return Compare(Json.Array(xv), Json.Array(yv));
        }

        private JsonComparer()
        {
        }
        
        private static JsonComparer _instance = new JsonComparer();
        public static JsonComparer Instance = _instance;
    }

    public class Json
    {
        public JsonValueType Type { get; init; }

        internal bool GetBool()
            => Type != JsonValueType.Null && Type != JsonValueType.False;

        public virtual IEnumerable<Json> EnumerateArray() => throw new NotImplementedException();

        public virtual IEnumerable<JsonProperty> EnumerateObject() => throw new NotImplementedException();

        public virtual double GetNumber() => throw new NotImplementedException();

        public virtual string GetString() => throw new NotImplementedException();

        public virtual Json GetElementAt(int index) => throw new NotImplementedException();
        public virtual Json SetElementAt(int index, Json value) => throw new NotImplementedException();

        public virtual Json GetElementAt(string key) => throw new NotImplementedException();
        public virtual Json SetElementAt(string key, Json value) => throw new NotImplementedException();

        public virtual int GetLength() => throw new NotImplementedException();

        public override string ToString() => JsonPrinter.AsString(this);

        public bool DeepEqual(Json other)
        {
            if (Type != other.Type)
            {
                return false;
            }
            return Type switch {
                JsonValueType.Undefined => true,
                JsonValueType.Null => true,
                JsonValueType.True => true,
                JsonValueType.False => true,
                JsonValueType.Number => GetNumber() == other.GetNumber(),
                JsonValueType.String => GetString() == other.GetString(),
                JsonValueType.Array => DeepEqualArray(other),
                JsonValueType.Object => DeepEqualObject(other),
                _ => throw new InvalidOperationException()
            };
        }

        private bool DeepEqualArray(Json other)
        {
            if (GetLength() != other.GetLength())
            {
                return false;
            }
            for (int i = 0; i < GetLength(); i++) 
            {
                if (!GetElementAt(i).DeepEqual(other.GetElementAt(i)))
                {
                    return false;
                }
            }
            return true;
        }

        private bool DeepEqualObject(Json other)
        {
            if (GetLength() != other.GetLength())
            {
                return false;
            }
            foreach (var kv in EnumerateObject())
            {
                if (!kv.Value.DeepEqual(other.GetElementAt(kv.Key)))
                {
                    return false;
                }
            }
            return true;
        }

        private static Json _undefined = new Json { Type = JsonValueType.Undefined };
        public static Json Undefined => _undefined;
        private static Json _null = new Json { Type = JsonValueType.Null };
        public static Json Null => _null;
        private static Json _true = new Json { Type = JsonValueType.True };
        public static Json True => _true;
        private static Json _false = new Json { Type = JsonValueType.False };
        public static Json False => _false;
        private static Json _zero = new JsonNumber(0);
        private static Json Zero => _zero;
        private static Json _one = new JsonNumber(1);
        private static Json One => _one;
        private static Json _emptyString = new JsonString("");
        private static Json EmptyString => _emptyString;
        private static Json _emptyArray = new JsonArray(Enumerable.Empty<Json>());
        private static Json EmptyArray => _emptyArray;
        private static Json _emptyObject = new JsonObject(Enumerable.Empty<JsonProperty>());
        private static Json EmptyObject => _emptyObject;

        public static Json Number(double value) =>
            value switch {
                0 => Zero,
                1 => One,
                _ => new JsonNumber(value)
            };
        
        public static Json String(string str) =>
            str switch {
                "" => EmptyString,
                _ => new JsonString(str)
            };
        
        public static Json ArrayParams(params Json[] args) => Array(args);

        public static Json Array(IEnumerable<Json> args)
        {
            var argsArray = args.ToArray();
            return argsArray.Length switch {
                0 => EmptyArray,
                _ => new JsonArray(argsArray)
            };
        }

        public static Json ObjectParams(params JsonProperty[] args) => Object(args);

        public static Json Object(IEnumerable<JsonProperty> args)
        {
            var argsArray = args.ToArray();
            return argsArray.Count() switch {
                0 => EmptyObject,
                _ => new JsonObject(argsArray)
            };
        }

        public static Json Bool(bool value) => value ? Json.True : Json.False;
    }

    class JsonNumber : Json
    {
        double _value;

        public JsonNumber(double value)
        {
            _value = value;
            Type = JsonValueType.Number;
        }

        public override double GetNumber()
            => _value;
    }

    class JsonString : Json
    {
        string _value;

        public JsonString(string value)
        {
            _value = value;
            Type = JsonValueType.String;
        }

        public override string GetString() => _value;

        public override int GetLength() => _value.Length;
    }

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
            index = index >= 0 ? index : _values.Count + index;
            if (index < 0 || index >= _values.Count)
            {
                return Json.Null;
            }
            else
            {
                return _values[index];
            }
        }

        public override int GetLength() => _values.Count;

        public override Json SetElementAt(int index, Json value)
            => new JsonArray(_values.SetItem(index, value));
    }

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
                return Json.Null;
            }
        }

        public override int GetLength() => _values.Count;

        public override Json SetElementAt(string key, Json value)
            => new JsonObject(_values.SetItem(key, value));
    }
}
