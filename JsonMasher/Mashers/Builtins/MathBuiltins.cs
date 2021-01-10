using System;
using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public static class MathBuiltins
    {
        public static Builtin Function_1(string functionName, Func<double, double> mathFunction)
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

        public static Builtin Function_2(string functionName, Func<double, double, double> mathFunction)
        {
            return new Builtin((List<IJsonMasherOperator> mashers, Json json, IMashContext context) =>
            {
                IEnumerable<Json> f()
                {
                    foreach (var value1 in mashers[0].Mash(json, context))
                    {
                        if (value1 != null && value1.Type == JsonValueType.Number)
                        {
                            foreach (var value2 in mashers[1].Mash(json, context))
                            {
                                if (value2 != null && value2.Type == JsonValueType.Number)
                                {
                                    yield return Json.Number(mathFunction(value1.GetNumber(), value2.GetNumber()));
                                }
                                else
                                {
                                    throw context.Error(
                                        $"Function {functionName} must be applied to numbers, not {value2?.Type}.",
                                        value2);
                                }
                            }
                        }
                        else
                        {
                            throw context.Error(
                                $"Function {functionName} must be applied to numbers, not {value1?.Type}.",
                                value1);
                        }
                    }
                }
                return f();
            }, 2);
        }
    }
}
