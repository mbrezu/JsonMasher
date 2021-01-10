using System;
using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public static class MathBuiltins
    {
        public static Builtin Function(string functionName, Func<double, double> mathFunction)
        {
            return new Builtin((List<IJsonMasherOperator> mashers, Json json, IMashContext context) =>
            {
                if (json != null && json.Type == JsonValueType.Number)
                {
                    return Json.Number(mathFunction(json.GetNumber())).AsEnumerable();
                }
                else
                {
                    throw context.Error(
                        $"Function must {functionName} be applied to numbers, not {json?.Type}.",
                        json);
                }
            }, 0);
        }
    }
}
