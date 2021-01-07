using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Tonumber
    {
        private static Builtin _builtin = new Builtin(Function, 0);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
            => json.Type switch {
                JsonValueType.String => TryParse(json.GetString(), context),
                _ => throw context.Error($"Cannot parse {json.Type} as number.", json)
            };

        private static IEnumerable<Json> TryParse(string value, IMashContext context)
        {
            if (double.TryParse(value, out double number))
            {
                return Json.Number(number).AsEnumerable();
            }
            else
            {
                throw context.Error($"Can't parse \"{value}\" as number.");
            }
        }

        public static Builtin Builtin => _builtin;
    }
}
