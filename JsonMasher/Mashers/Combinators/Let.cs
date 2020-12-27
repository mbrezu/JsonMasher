using System.Collections.Generic;

namespace JsonMasher.Mashers.Combinators
{
    public class Let : IJsonMasherOperator
    {
        public string Name { get; init; }
        public IJsonMasherOperator Value { get; init; }
        public IJsonMasherOperator Body { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            context.PushEnvironmentFrame();
            foreach (var jsonValue in Value.Mash(json, context))
            {
                context.SetVariable(Name, jsonValue);
                foreach (var result in Body.Mash(json, context))
                {
                    yield return result;
                }
            }
            context.PopEnvironmentFrame();
        }
    }
}
