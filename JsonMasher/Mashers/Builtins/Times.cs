using System;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Times
    {
        public static Builtin Builtin = Utils.MakeBinaryBuiltin(Operator);

        static Json Operator(Json t1, Json t2, IMashContext context)
            => (t1.Type, t2.Type) switch
            {
                (JsonValueType.Number, JsonValueType.Number)
                    => Json.Number(t1.GetNumber() * t2.GetNumber()),
                (JsonValueType.String, JsonValueType.Number)
                    => StringTimesNumber(t1.GetString(), (int)t2.GetNumber()),
                (JsonValueType.Number, JsonValueType.String)
                    => StringTimesNumber(t2.GetString(), (int)t1.GetNumber()),
                (JsonValueType.Object, JsonValueType.Object)
                    => MergeDictionariesRecursively(t1, t2),
                _ => throw context.Error($"Can't multiply {t1.Type} and {t2.Type}.", t1, t2)
            };

        private static Json StringTimesNumber(string str, int times)
        {
            var newString = str.Repeat(times);
            if (newString == null)
            {
                return Json.Null;
            }
            else
            {
                return Json.String(newString);
            }
        }

        private static Json MergeDictionariesRecursively(Json t1, Json t2)
        {
            foreach (var kv2 in t2.EnumerateObject())
            {
                var v1 = t1.GetElementAt(kv2.Key);
                if (v1.Type == JsonValueType.Object && kv2.Value.Type == JsonValueType.Object)
                {
                    t1 = t1.SetElementAt(kv2.Key, MergeDictionariesRecursively(v1, kv2.Value));
                }
                else
                {
                    t1 = t1.SetElementAt(kv2.Key, kv2.Value);
                }
            }
            return t1;
        }
    }
}
