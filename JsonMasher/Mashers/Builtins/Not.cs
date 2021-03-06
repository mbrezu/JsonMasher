using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Not
    {
        private static Builtin _builtin 
            = new Builtin(
                (mashers, json, context) => Json.Bool(!json.GetBool()).AsEnumerable(),
                0);
        
        public static Builtin Builtin => _builtin;
    }
}
