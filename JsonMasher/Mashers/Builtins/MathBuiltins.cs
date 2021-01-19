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
                EnsureNumber(json, context, functionName);
                return Json.Number(mathFunction(json.GetNumber())).AsEnumerable();
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
                        EnsureNumber(value1, context, functionName);
                        foreach (var value2 in mashers[1].Mash(json, context))
                        {
                            EnsureNumber(value2, context, functionName);
                            yield return Json.Number(mathFunction(value1.GetNumber(), value2.GetNumber()));
                        }
                    }
                }
                return f();
            }, 2);
        }

        public static Builtin Function_3(
            string functionName, Func<double, double, double, double> mathFunction)
        {
            return new Builtin((List<IJsonMasherOperator> mashers, Json json, IMashContext context) =>
            {
                IEnumerable<Json> f()
                {
                    foreach (var value1 in mashers[0].Mash(json, context))
                    {
                        EnsureNumber(value1, context, functionName);
                        foreach (var value2 in mashers[1].Mash(json, context))
                        {
                            EnsureNumber(value2, context, functionName);
                            foreach (var value3 in mashers[2].Mash(json, context))
                            {
                                EnsureNumber(value3, context, functionName);
                                yield return Json.Number(mathFunction(
                                    value1.GetNumber(), value2.GetNumber(), value3.GetNumber()));
                            }
                        }
                    }
                }
                return f();
            }, 3);
        }

        private static void EnsureNumber(Json value, IMashContext context, string functionName)
        {
            if (value != null && value.Type == JsonValueType.Number)
            {
                return;
            }
            throw context.Error(
                $"Function {functionName} must be applied to numbers, not {value?.Type}.",
                value);
        }
    }
}
