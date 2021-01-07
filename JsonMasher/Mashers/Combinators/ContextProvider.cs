using System.Collections.Generic;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Mashers.Combinators
{
    public class ContextProvider : IJsonMasherOperator, IPathGenerator
    {

        public IMashContext Context { get; init; }
        public IJsonMasherOperator Body { get; init; }
        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => Body.Mash(json, Context); // <-- ignore `context` passed 
                                         // as parameter and use the property instead
                                         // (thus providing the context).

        public IEnumerable<PathAndValue> GeneratePaths(
            JsonPath pathSoFar, Json json, IMashContext context)
        {
            if (Body is IPathGenerator pathGenerator)
            {
                return pathGenerator.GeneratePaths(pathSoFar, json, Context);
            }
            else
            {
                throw context.PushStack(Body).Error("Not a path expression.");
            }
        }

        public static IJsonMasherOperator Wrap(IMashContext context, IJsonMasherOperator body)
        {
            if (body is ContextProvider contextProvider)
            {
                return new ContextProvider {
                    Context = context,
                    Body = contextProvider.Body
                };
            }
            else
            {
                return new ContextProvider {
                    Context = context,
                    Body = body
                };
            }
        }

    }
}
