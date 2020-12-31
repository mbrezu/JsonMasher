using System;
using System.Collections.Generic;
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

        public virtual Json GetElementAt(string key) => throw new NotImplementedException();

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

        public static Json Array(IEnumerable<Json> args) =>
            args.Count() switch {
                0 => EmptyArray,
                _ => new JsonArray(args)
            };

        public static Json ObjectParams(params JsonProperty[] args) => Object(args);

        public static Json Object(IEnumerable<JsonProperty> args) =>
            args.Count() switch {
                0 => EmptyObject,
                _ => new JsonObject(args)
            };
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
        List<Json> _values;

        public JsonArray(IEnumerable<Json> values)
        {
            _values = new();
            _values.AddRange(values);
            Type = JsonValueType.Array;
        }

        public override IEnumerable<Json> EnumerateArray() => _values;

        public override Json GetElementAt(int index) => 
            index >= 0 ? _values[index] : _values[_values.Count + index];

        public override int GetLength() => _values.Count;
    }

    class JsonObject : Json
    {
        Dictionary<string, Json> _values;

        public JsonObject(IEnumerable<JsonProperty> values)
        {
            _values = new();
            foreach (var kv in values)
            {
                _values[kv.Key] = kv.Value;
            }
            Type = JsonValueType.Object;
        }

        public override IEnumerable<JsonProperty> EnumerateObject()
        {
            foreach (var kv in _values)
            {
                yield return new JsonProperty(kv.Key, kv.Value);
            }
        }

        public override Json GetElementAt(string key) => _values[key];

        public override int GetLength() => _values.Count;
    }
}
