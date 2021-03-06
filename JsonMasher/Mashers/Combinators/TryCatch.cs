using System.Collections.Generic;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class TryCatch : IJsonMasherOperator
    {
        public IJsonMasherOperator TryBody { get; set; }
        public IJsonMasherOperator CatchBody { get; set; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            context = context.PushStack(this);
            context.Tick();
            var result = new List<Json>();
            try
            {
                foreach (var value in TryBody.Mash(json, context))
                {
                    result.Add(value);
                }
            }
            catch (JsonMasherException ex)
            {
                if (CatchBody != null)
                {
                    foreach (var value in CatchBody.Mash(Json.String(ex.Message), context))
                    {
                        result.Add(value);
                    }
                }
            }
            return result;
        }
    }
}
