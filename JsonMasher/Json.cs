using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JsonMasher
{
    public record JsonProperty(string Key, Json Value);
    public enum JsonValueType : byte
    {
        Undefined = 0,
        Object = 1,
        Array = 2,
        String = 3,
        Number = 4,
        True = 5,
        False = 6,
        Null = 7
    }

    public class Json
    {
        public JsonValueType Type { get; init; }

        internal bool GetBool()
            => Type switch {
                JsonValueType.True => true,
                JsonValueType.False => false,
                _ => throw new InvalidOperationException()
            };

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

        public static Json Undefined => new Json { Type = JsonValueType.Undefined };
        public static Json Null => new Json { Type = JsonValueType.Null };
        public static Json True => new Json { Type = JsonValueType.True };
        public static Json False => new Json { Type = JsonValueType.False };
        private static Json Zero => new JsonNumber(0);
        private static Json One => new JsonNumber(1);
        private static Json EmptyString => new JsonString("");
        private static Json EmptyArray => new JsonArray(Enumerable.Empty<Json>());
        private static Json EmptyObject => new JsonObject(Enumerable.Empty<JsonProperty>());

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

        public override bool Equals(object obj)
        {
            if (obj is Json other)
            {
                return DeepEqual(other);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() => ToString().GetHashCode();
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
