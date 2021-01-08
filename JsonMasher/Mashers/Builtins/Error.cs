using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Error
    {
        private static Builtin _builtin 
            = new Builtin((mashers, json, context) =>
        {
            if (json != null && json.Type == JsonValueType.String)
            {
                throw context.Error(json.GetString(), json);
            }
            else
            {
                throw context.Error(
                    $"Argument to error must be string, not {json?.Type}.", json);
            }
        }, 0);

        public static Builtin Builtin => _builtin;
    }
}
