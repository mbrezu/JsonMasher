using System;
using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Contains
    {
        private static Builtin _builtin = new Builtin(Function, 1);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context)
        {
            foreach (var value in mashers[0].Mash(json, context))
            {
                yield return (json.Type, value.Type) switch {
                    (JsonValueType.String, JsonValueType.String)
                        => Json.Bool(json.GetString().IndexOf(value.GetString()) != -1),
                    (JsonValueType.Array, JsonValueType.Array)
                        => Json.Bool(ArrayContains(json, value)),
                    (JsonValueType.Object, JsonValueType.Object)
                        => Json.Bool(ObjectContains(json, value)),
                    _ => throw context.Error($"Can't check if {json.Type} contains {value.Type}.", json, value)
                };
            }
        }

        private static bool ArrayContains(Json json, Json value)
        {
            foreach (var elementToFind in value.EnumerateArray())
            {
                var found = false;
                foreach (var element in json.EnumerateArray())
                {
                    if (element.DeepEqual(elementToFind))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ObjectContains(Json json, Json value)
        {
            foreach (var kvToFind in value.EnumerateObject())
            {
                if (!json.ContainsKey(kvToFind.Key))
                {
                    return false;
                }
                if (!json.GetElementAt(kvToFind.Key).DeepEqual(kvToFind.Value))
                {
                    return false;
                }
            }
            return true;
        }

        public static Builtin Builtin => _builtin;
    }
}
