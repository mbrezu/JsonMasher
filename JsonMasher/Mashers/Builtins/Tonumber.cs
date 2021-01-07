using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Tonumber
    {
        private static Builtin _builtin = new Builtin(Function, 0);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
            => json.Type switch {
                JsonValueType.String => TryParse(json.GetString(), context, stack),
                _ => throw context.Error($"Cannot parse {json.Type} as number.", stack, json)
            };

        private static IEnumerable<Json> TryParse(string value, IMashContext context, IMashStack stack)
        {
            if (double.TryParse(value, out double number))
            {
                return Json.Number(number).AsEnumerable();
            }
            else
            {
                throw context.Error($"Can't parse \"{value}\" as number.", stack);
            }
        }

        public static Builtin Builtin => _builtin;
    }
}
