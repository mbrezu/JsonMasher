using System;
using System.Collections.Generic;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Recurse
    {
        private static Builtin _builtin = new Builtin(Function , 0);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
                => RecurseJson(json, context, stack);

        private static IEnumerable<Json> RecurseJson(
            Json value, IMashContext context, IMashStack stack)
        {
            context.Tick(stack);
            return value.Type switch
            {
                JsonValueType.Null
                    or JsonValueType.True
                    or JsonValueType.False
                    or JsonValueType.Number
                    or JsonValueType.String
                        => value.AsEnumerable(),
                JsonValueType.Array => RecurseArray(value, context, stack),
                JsonValueType.Object => RecurseObject(value, context,stack),
                _ => throw new InvalidOperationException()
            };
        }

        private static IEnumerable<Json> RecurseArray(Json value, IMashContext context, IMashStack stack)
        {
            yield return value;
            foreach (var element in value.EnumerateArray())
            {
                foreach (var recurseElement in RecurseJson(element, context, stack))
                {
                    yield return recurseElement;
                }
            }
        }

        private static IEnumerable<Json> RecurseObject(Json value, IMashContext context, IMashStack stack)
        {
            yield return value;
            foreach (var kv in value.EnumerateObject())
            {
                foreach (var recurseElement in RecurseJson(kv.Value, context, stack))
                {
                    yield return recurseElement;
                }
            }
        }

        public static Builtin Builtin => _builtin;
    }
}
