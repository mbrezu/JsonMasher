using System;
using System.Collections.Generic;
using System.Globalization;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class DateFunctions
    {
        private static Builtin _fromDateBuiltin = new Builtin(FromDate_Function, 0);
        private static Builtin _toDateBuiltin = new Builtin(ToDate_Function, 0);
        private static Builtin _nowBuiltin = new Builtin(Now_Function, 0);
        private static Builtin _strftimeBuiltin = new Builtin(Strftime_Function, 1);
        private static Builtin _strptimeBuiltin = new Builtin(Strptime_Function, 1);

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

        private static IEnumerable<Json> Strftime_Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            if (json == null || json.Type != JsonValueType.Number)
            {
                throw context.Error(
                        $"strftime: need a number of seconds, not {json?.Type}.", json);
            }
            foreach (var formatString in mashers[0].Mash(json, context))
            {
                if (formatString == null || formatString.Type != JsonValueType.String)
                {
                    throw context.Error(
                        $"strftime: need a format string, not {formatString?.Type}.", formatString);
                }
                var date = _epoch.AddSeconds(json.GetNumber());
                var result = date.ToString(formatString.GetString(), CultureInfo.InvariantCulture);
                yield return Json.String(result);
            }
        }

        private static IEnumerable<Json> Strptime_Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            if (json == null || json.Type != JsonValueType.String)
            {
                throw context.Error(
                        $"strptime: need string representing a date, not {json?.Type}.", json);
            }
            foreach (var formatString in mashers[0].Mash(json, context))
            {
                if (formatString == null || formatString.Type != JsonValueType.String)
                {
                    throw context.Error(
                        $"strptime: need a format string, not {formatString?.Type}.", formatString);
                }
                if (DateTimeOffset.TryParseExact(
                    json.GetString(), formatString.GetString(), null, DateTimeStyles.None, out var result))
                {
                    yield return Json.Number((result - _epoch).TotalSeconds);
                }
                else
                {
                    throw context.Error(
                        $"strptime: Can't parse '{json.GetString()}' using '{formatString.GetString()}'.",
                        json,
                        formatString);
                }
            }
        }

        public static Builtin FromDate => _fromDateBuiltin;
        public static Builtin ToDate => _toDateBuiltin;
        public static Builtin Now => _nowBuiltin;
        public static Builtin Strftime => _strftimeBuiltin;
        public static Builtin Strptime => _strptimeBuiltin;
    }
}
