using System.Linq;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Empty
    {
        private static Builtin _builtin 
            = new Builtin((mashers, json, context) => Enumerable.Empty<Json>() , 0);
        public static Builtin Builtin => _builtin;
    }
}
