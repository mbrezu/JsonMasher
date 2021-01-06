using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.JsonRepresentation
{
    public record JsonProperty(string Key, Json Value);

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

        public virtual JsonPathPart GetPathPart() => throw new NotImplementedException();

        public virtual Json DelElementAt(int index) => throw new NotImplementedException();

        public virtual Json GetSliceAt(int start, int end) => throw new NotImplementedException();
        public virtual Json SetSliceAt(int start, int end, Json value) => throw new NotImplementedException();
        public virtual Json DelSliceAt(int start, int end) => throw new NotImplementedException();

        public virtual Json GetElementAt(string key) => throw new NotImplementedException();
        public virtual Json SetElementAt(string key, Json value) => throw new NotImplementedException();
        public virtual Json DelElementAt(string key) => throw new NotImplementedException();

        public virtual bool ContainsKey(int index) => throw new NotImplementedException();
        public virtual bool ContainsKey(string key) => throw new NotImplementedException();

        public virtual int GetLength() => throw new NotImplementedException();

        public virtual Json TransformByPath(
            JsonPath path, Func<Json, Json> transformer, Func<Json, JsonPathPart, Exception> onError)
        {
            if (path == JsonPath.Empty)
            {
                return transformer(this);
            }
            else
            {
                var key = path.Parts.First();
                var restOfPath = path.WithoutFirstPart;
                if (key is IntPathPart ip)
                {
                    var json = this;
                    if (json == Json.Null)
                    {
                        json = Json.Array(Enumerable.Repeat(Json.Null, ip.Value + 1));
                    }
                    return TransformArrayElement(json, transformer, onError, restOfPath, ip);
                }
                else if (key is StringPathPart sp)
                {
                    var json = this;
                    if (json == Json.Null)
                    {
                        json = Json.ObjectParams(new JsonProperty(sp.Value, Json.Null));
                    }
                    return TransformObjectKey(json, transformer, onError, restOfPath, sp);
                }
                else if (key is SlicePathPart slicePart)
                {
                    var json = this;
                    if (json == Json.Null)
                    {
                        json = Json.ArrayParams();
                    }
                    return TransformArraySlice(json, transformer, onError, restOfPath, slicePart);
                }
                else
                {
                    throw onError(this, key);
                }
            }
        }

        private static Json TransformArrayElement(
            Json json,
            Func<Json, Json> transformer,
            Func<Json, JsonPathPart, Exception> onError,
            JsonPath restOfPath,
            IntPathPart ip)
        {
            var element = json.GetElementAt(ip.Value);
            var transformed = element.TransformByPath(restOfPath, transformer, onError);
            if (transformed == element)
            {
                return json;
            }
            else if (transformed == null)
            {
                return json.DelElementAt(ip.Value);
            }
            else
            {
                return json.SetElementAt(ip.Value, transformed);
            }
        }

        private static Json TransformObjectKey(
            Json json,
            Func<Json, Json> transformer,
            Func<Json, JsonPathPart, Exception> onError,
            JsonPath restOfPath,
            StringPathPart sp)
        {
            var element = json.GetElementAt(sp.Value);
            var transformed = element.TransformByPath(restOfPath, transformer, onError);
            if (transformed == element)
            {
                return json;
            }
            else if (transformed == null)
            {
                return json.DelElementAt(sp.Value);
            }
            else
            {
                return json.SetElementAt(sp.Value, transformed);
            }
        }

        private static Json TransformArraySlice(
            Json json,
            Func<Json, Json> transformer,
            Func<Json, JsonPathPart, Exception> onError,
            JsonPath restOfPath,
            SlicePathPart slicePart)
        {
            var element = json.GetSliceAt(slicePart.Start, slicePart.End);
            var transformed = element.TransformByPath(restOfPath, transformer, onError);
            if (transformed == element)
            {
                return json;
            }
            else if (transformed == null)
            {
                return json.DelSliceAt(slicePart.Start, slicePart.End);
            }
            else
            {
                return json.SetSliceAt(slicePart.Start, slicePart.End, transformed);
            }
        }

        public override string ToString() => JsonPrinter.AsString(this);

        public bool DeepEqual(Json other)
        {
            if (Type != other.Type)
            {
                return false;
            }
            return Type switch
            {
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
            value switch
            {
                0 => Zero,
                1 => One,
                _ => new JsonNumber(value)
            };

        public static Json String(string str) =>
            str switch
            {
                "" => EmptyString,
                _ => new JsonString(str)
            };

        public static Json ArrayParams(params Json[] args) => Array(args);

        public static Json Array(IEnumerable<Json> args)
        {
            var argsArray = args.ToArray();
            return argsArray.Length switch
            {
                0 => EmptyArray,
                _ => new JsonArray(argsArray)
            };
        }

        public static Json ObjectParams(params JsonProperty[] args) => Object(args);

        public static Json Object(IEnumerable<JsonProperty> args)
        {
            var argsArray = args.ToArray();
            return argsArray.Count() switch
            {
                0 => EmptyObject,
                _ => new JsonObject(argsArray)
            };
        }

        public static Json Bool(bool value) => value ? True : False;
    }
}
