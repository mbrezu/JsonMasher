using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class RegexMatch
    {
        private static Builtin _builtin = new Builtin(Function, 3);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            if (json == null || json.Type != JsonValueType.String)
            {
                throw context.Error($"The input must be a string, not {json?.Type}.", json);
            }
            foreach (var regex in mashers[0].Mash(json, context))
            {
                if (regex == null || regex.Type != JsonValueType.String)
                {
                    throw context.Error($"The regex must be a string, not {regex?.Type}.", regex);
                }
                foreach (var flags in mashers[1].Mash(json, context))
                {
                    string flagsValue = flags.Type == JsonValueType.String ? flags.GetString() : "";
                    foreach (var onlyTest in mashers[2].Mash(json, context))
                    {
                        var matches = GetMatches(
                            json.GetString(), regex.GetString(), flagsValue, onlyTest.GetBool());
                        foreach (var match in matches)
                        {
                            yield return match;
                        }
                    }
                }
            }
        }

        private static IEnumerable<Json> GetMatches(string str, string regex, string flags, bool onlyTest)
        {
            var global = flags.Contains("g");
            RegexOptions options = RegexOptions.None;
            if (flags.Contains("i"))
            {
                options |= RegexOptions.IgnoreCase;
            }
            if (flags.Contains("m"))
            {
                options |= RegexOptions.Singleline;
            }
            if (!flags.Contains("s"))
            {
                options |= RegexOptions.Multiline;
            }
            if (flags.Contains("x"))
            {
                options |= RegexOptions.IgnorePatternWhitespace;
            }
            bool filterEmptyMatches = flags.Contains("n");
            var re = new Regex(regex, options);
            if (onlyTest)
            {
                yield return Json.Bool(re.Match(str).Success);
            }
            else if (global)
            {
                var result = new List<Json>();
                foreach (Match match in re.Matches(str))
                {
                    if (match.Success && (!filterEmptyMatches || match.Length > 0))
                    {
                        result.Add(JsonFromMatch(match));
                    }
                }
                yield return Json.Array(result);
            }
            else
            {
                var match = re.Match(str);
                if (match.Success && (!filterEmptyMatches || match.Length > 0))
                {
                    yield return Json.ArrayParams(JsonFromMatch(match));
                }
            }
        }

        private static Json JsonFromMatch(Match match)
            => Json.ObjectParams(
                new JsonProperty("offset", Json.Number(match.Index)),
                new JsonProperty("length", Json.Number(match.Length)),
                new JsonProperty("string", Json.String(match.Value)),
                new JsonProperty(
                    "captures", Json.Array(((IEnumerable<Group>)match.Groups).Skip(1).Select(JsonFromGroup))));

        private static Json JsonFromGroup(Group group)
            => Json.ObjectParams(
                new JsonProperty("offset", Json.Number(group.Index)),
                new JsonProperty("length", Json.Number(group.Length)),
                new JsonProperty("string", Json.String(group.Value)),
                new JsonProperty("name", Json.String(group.Name)));

        public static Builtin Builtin => _builtin;
    }
}
