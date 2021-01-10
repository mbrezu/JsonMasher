using System;
using System.Collections.Generic;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public static class StringBuiltins
    {
        public static Builtin Function_1(string functionName, Func<string, Json> function)
        {
            return new Builtin((List<IJsonMasherOperator> mashers, Json json, IMashContext context) =>
            {
                if (json != null && json.Type == JsonValueType.String)
                {
                    return function(json.GetString()).AsEnumerable();
                }
                else
                {
                    throw context.Error(
                        $"Function {functionName} must be applied to strings, not {json?.Type}.",
                        json);
                }
            }, 0);
        }
        public static Builtin Function_2(string functionName, Func<string, string, Json> function)
        {
            return new Builtin((List<IJsonMasherOperator> mashers, Json json, IMashContext context) =>
            {
                IEnumerable<Json> f()
                {
                    if (json != null && json.Type == JsonValueType.String)
                    {
                        foreach (var value in mashers[0].Mash(json, context))
                        {
                            if (value != null && value.Type == JsonValueType.String)
                            {
                                yield return function(json.GetString(), value.GetString());
                            }
                            else
                            {
                                throw context.Error(
                                    $"Function {functionName} must be applied to strings, not {value?.Type}.",
                                    value);
                            }
                        }
                    }
                    else
                    {
                        throw context.Error(
                            $"Function {functionName} must be applied to strings, not {json?.Type}.",
                            json);
                    }
                }
                return f();
            }, 1);
        }
    }
}
