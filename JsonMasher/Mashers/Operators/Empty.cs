using System;
using System.Linq;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Operators
{
    public class Empty
    {
        private static Builtin _builtin 
            = new Builtin((mashers, json, context) => Enumerable.Empty<Json>() , 0);
        public static Builtin Builtin => _builtin;
    }
}
