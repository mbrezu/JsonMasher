using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JsonMasher
{
    public interface JsonPathPart {};
    public record StringPathPart(string Value): JsonPathPart;
    public record IntPathPart(int Value): JsonPathPart;
    public record SlicePathPart(int Start, int End): JsonPathPart;
    public class JsonPath
    {
        ImmutableList<JsonPathPart> _parts;

        public IEnumerable<JsonPathPart> Parts => _parts;

        public JsonPath(IEnumerable<JsonPathPart> parts) => _parts = FromParts(parts);

        public JsonPath(params JsonPathPart[] parts) => _parts = FromParts(parts);

        private JsonPath(ImmutableList<JsonPathPart> parts) => _parts = parts;

        private static ImmutableList<JsonPathPart> FromParts(IEnumerable<JsonPathPart> parts)
            => ImmutableList<JsonPathPart>.Empty.AddRange(parts);
        
        public JsonPath Extend(JsonPathPart part) => new JsonPath(_parts.Add(part));

        public Json ToJsonArray() => Json.Array(_parts.Select(p => p switch {
            StringPathPart sp => Json.String(sp.Value),
            IntPathPart ip => Json.Number(ip.Value),
            SlicePathPart slicePathPart => Json.ObjectParams(
                new JsonProperty("start", Json.Number(slicePathPart.Start)),
                new JsonProperty("end", Json.Number(slicePathPart.End))),
            _ => throw new InvalidOperationException()
        }));

        public JsonPath WithoutFirstPart
            => _parts.Count == 1 ? JsonPath.Empty : new JsonPath(_parts.RemoveAt(0));

        private static JsonPath _empty = new JsonPath();
        public static JsonPath Empty = _empty;
    }

}
