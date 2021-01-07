using System.Linq;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Error
    {
        private static Builtin _builtin 
            = new Builtin((mashers, json, context) =>
        {
            var value = mashers[0].Mash(json, context).FirstOrDefault();
            if (value != null && value.Type == JsonValueType.String)
            {
                throw context.Error(value.GetString(), value);
            }
            else
            {
                throw context.Error(
                    $"Argument to error must be string, not {value?.Type}.", value);
            }
        }, 1);

        public static Builtin Builtin => _builtin;
    }
}
