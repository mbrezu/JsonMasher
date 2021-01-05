using System;
using System.Linq;
using System.Web;
using FancyPen;

namespace JsonMasher.JsonRepresentation
{
    public static class JsonPrinter
    {
        private static BracedListConfig _arrayConfig = new BracedListConfig("[", "]");
        private static BracedListConfig _objectConfig = new BracedListConfig("{", "}");

        public static string AsString(Json value)
        {
            var renderer = new Renderer(80);
            var doc = Print(value);
            return renderer.Render(doc);
        }

        private static Document Print(Json value) => value.Type switch
        {
            JsonValueType.Array => PrintArray(value),
            JsonValueType.Object => PrintObject(value),
            JsonValueType.String => JsonEscape(value.GetString()),
            JsonValueType.Number => value.GetNumber().ToString(),
            JsonValueType.True => "true",
            JsonValueType.False => "false",
            JsonValueType.Null => "null",
            _ => throw new NotImplementedException()
        };

        private static Document PrintArray(Json json)
        {
            var children = json
                .EnumerateArray()
                .Select(element => Print(element))
                .ToArray();
            return CreateBracedList(_arrayConfig, children);
        }

        private static Document CreateBracedList(
            BracedListConfig listConfig,
            Document[] children) => Utils.BracedListIndentAmount(listConfig, 4, children);

        private static Document PrintObject(Json json)
        {
            var children = json
                .EnumerateObject()
                .Select(element => PrintKeyValue(element))
                .ToArray();
            return CreateBracedList(_objectConfig, children);
        }

        private static string JsonEscape(string str)
            => $"\"{HttpUtility.JavaScriptStringEncode(str)}\"";

        private static Document PrintKeyValue(JsonProperty element)
            => Document.Concat(JsonEscape(element.Key), ": ", Print(element.Value));
    }
}
