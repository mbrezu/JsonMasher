using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JsonMasher.JsonRepresentation
{
    public interface JsonPathPart { 
        Json ToJson();
    };
    public record StringPathPart(string Value) : JsonPathPart
    {
        public override string ToString() => $"string \"{Value}\"";
        public Json ToJson() => Json.String(Value);
    }

    public record IntPathPart(int Value) : JsonPathPart
    {
        public override string ToString() => $"int {Value}";
        public Json ToJson() => Json.Number(Value);
    }
    public record SlicePathPart(int Start, int End) : JsonPathPart
    {
        public override string ToString() => $"slice {Start}:{End}";
        public Json ToJson() => Json.ObjectParams(
                new JsonProperty("start", Json.Number(Start)),
                new JsonProperty("end", Json.Number(End)));
    }

    public class JsonPath
    {
        ImmutableList<JsonPathPart> _parts;

        public IEnumerable<JsonPathPart> Parts => _parts;

        private JsonPath(ImmutableList<JsonPathPart> parts) => _parts = parts;

        public static JsonPath FromParts(IEnumerable<JsonPathPart> parts)
        {
            if (parts.Any())
            {
                if (parts is ImmutableList<JsonPathPart> il)
                {
                    return new JsonPath(il);
                }
                else
                {
                    return new JsonPath(ImmutableList<JsonPathPart>.Empty.AddRange(parts));
                }
            }
            else
            {
                return Empty;
            }
        }

        public JsonPath Extend(JsonPathPart part) => FromParts(_parts.Add(part));

        public Json ToJsonArray() => Json.Array(_parts.Select(p => p.ToJson()));

        public JsonPath WithoutFirstPart
            => _parts.Count == 1 ? Empty : FromParts(_parts.RemoveAt(0));

        private static JsonPath _empty = new JsonPath(ImmutableList<JsonPathPart>.Empty);
        public static JsonPath Empty = _empty;
    }

}
