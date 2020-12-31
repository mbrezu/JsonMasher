using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers.Builtins
{
    public class Not
    {
        private static Builtin _builtin 
            = new Builtin(
                (mashers, json, context, stack) =>
                {
                    if (json.Type != JsonValueType.True && json.Type != JsonValueType.False)
                    {
                        throw context.Error($"Can't 'not' a {json.Type}.", stack, json);
                    }
                    return Json.Bool(!json.GetBool()).AsEnumerable();
                },
                0);
        
        public static Builtin Builtin => _builtin;
    }
}
