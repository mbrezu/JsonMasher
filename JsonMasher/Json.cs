using System;
using System.Collections.Generic;

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

        public virtual IEnumerable<Json> EnumerateArray()
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<JsonProperty> EnumerateObject()
        {
            throw new NotImplementedException();
        }

        public virtual double GetNumber()
        {
            throw new NotImplementedException();
        }

        public virtual string GetString()
        {
            throw new NotImplementedException();
        }

        public virtual Json GetElementAt(int index)
        {
            throw new NotImplementedException();
        }

        public virtual Json GetElementAt(string key)
        {
            throw new NotImplementedException();
        }

        public static Json Undefined => new Json { Type = JsonValueType.Undefined };
        public static Json Null => new Json { Type = JsonValueType.Null };
        public static Json True => new Json { Type = JsonValueType.True };
        public static Json False => new Json { Type = JsonValueType.False };
        private static Json Zero => new JsonNumber(0);
        private static Json One => new JsonNumber(1);
        private static Json EmptyString => new JsonString("");
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
    }

    public class JsonNumber : Json
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

    public class JsonString : Json
    {
        string _value;

        public JsonString(string value)
        {
            _value = value;
            Type = JsonValueType.String;
        }

        public override string GetString()
            => _value;
    }

    public class JsonArray : Json
    {
        List<Json> _values;

        public JsonArray(IEnumerable<Json> values)
        {
            _values = new();
            _values.AddRange(values);
            Type = JsonValueType.Array;
        }

        public override IEnumerable<Json> EnumerateArray()
        {
            return _values;
        }

        public override Json GetElementAt(int index)
            => _values[index];
    }

    public class JsonObject : Json
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

        public override Json GetElementAt(string key)
            => _values[key];
    }
}
