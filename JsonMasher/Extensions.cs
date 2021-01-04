using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using JsonMasher.Mashers;
using JsonMasher.Mashers.Primitives;

namespace JsonMasher
{
    public static class Extensions
    {
        public static Json AsJson(this JsonDocument doc)
            => doc.RootElement.AsJson();

        public static Json AsJson(this JsonElement element)
            => element.ValueKind switch
            {
                JsonValueKind.Undefined => Json.Undefined,
                JsonValueKind.Null => Json.Null,
                JsonValueKind.True => Json.True,
                JsonValueKind.False => Json.False,
                JsonValueKind.Number => Json.Number(element.GetDouble()),
                JsonValueKind.String => Json.String(element.GetString()),
                JsonValueKind.Array => new JsonArray(
                    element.EnumerateArray().Select(elt => elt.AsJson())),
                JsonValueKind.Object => new JsonObject(
                    element
                    .EnumerateObject()
                    .Select(jp => new JsonProperty(jp.Name, jp.Value.AsJson()))),
                _ => throw new InvalidOperationException()
            };

        public static Json AsJson(this string json) => JsonDocument.Parse(json).AsJson();

        public static IEnumerable<Json> AsMultipleJson(this string json)
        {
            var result = new List<Json>();
            var inputBytes = UTF8Encoding.UTF8.GetBytes(json.Trim());
            int offset = 0;
            int length = inputBytes.Length;
            while (offset < inputBytes.Length)
            {
                var stream = new Utf8JsonReader(new ReadOnlySpan<byte>(inputBytes, offset, length));
                result.Add(JsonDocument.ParseValue(ref stream).AsJson());
                offset += (int)stream.BytesConsumed;
                length -= (int)stream.BytesConsumed;
            }
            return result;
        }

        public static IEnumerable<T> AsEnumerable<T>(this T value)
        {
            yield return value;
        }

        internal static IJsonMasherOperator Fold(
            this IEnumerable<IJsonMasherOperator> mashers,
            Func<IJsonMasherOperator, IJsonMasherOperator, IJsonMasherOperator> combiner)
        => mashers.Count() switch
            {
                0 => new Identity(),
                1 => mashers.First(),
                _ => combiner(mashers.First(), mashers.Skip(1).Fold(combiner))
            };

        public static string Repeat(this string str, int times)
        {
            if (times == 0)
            {
                return null;
            }
            var sb = new StringBuilder();
            for (int i = 0; i < times; i++)
            {
                sb.Append(str);
            }
            return sb.ToString();
        }

        public static string GetEnumDisplayName(this Enum value)
            => value.GetType().GetMember(value.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                ?.Name ?? value.ToString();
    }
}