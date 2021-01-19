using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class DateFunctions
    {
        private static Builtin _fromDateBuiltin = new Builtin(FromDate_Function, 0);
        private static Builtin _toDateBuiltin = new Builtin(ToDate_Function, 0);

        private static Builtin _nowBuiltin = new Builtin(Now_Function, 0);

        private static DateTimeOffset _epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private static IEnumerable<Json> FromDate_Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            if (json == null || json.Type != JsonValueType.String)
            {
                throw context.Error($"fromdateiso8601: Need a string, not {json?.Type}.", json);
            }
            if (DateTimeOffset.TryParse(json.GetString(), out var result))
            {
                yield return Json.Number((result - _epoch).TotalSeconds);
            }
            else
            {
                throw context.Error($"fromdateiso8601: Can't parse '{json.GetString()}' as ISO 8601 date.", json);
            }
        }

        private static IEnumerable<Json> ToDate_Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            if (json == null || json.Type != JsonValueType.Number)
            {
                throw context.Error($"todateiso8601: Need a number, not {json?.Type}.", json);
            }
            var date = _epoch.AddSeconds(json.GetNumber());
            yield return Json.String(date.ToString("o", CultureInfo.InvariantCulture));
        }

        private static IEnumerable<Json> Now_Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            var utcSecondsSinceEpoch = (DateTimeOffset.UtcNow - _epoch).TotalSeconds;
            yield return Json.Number(utcSecondsSinceEpoch);
        }

        public static Builtin FromDate => _fromDateBuiltin;
        public static Builtin ToDate => _toDateBuiltin;
        public static Builtin Now => _nowBuiltin;
    }
}
