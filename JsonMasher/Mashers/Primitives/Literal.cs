using System.Collections.Generic;

namespace JsonMasher.Mashers.Primitives
{
    public class Literal : IJsonMasherOperator
    {
        public Json Value { get; init; }

        public Literal()
        {
        }

        public Literal(Json value)
        {
            Value = value;
        }

        public Literal(double value)
        {
            Value = Json.Number(value);
        }

        public Literal(string value)
        {
            Value = Json.String(value);
        }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
            => Value.AsEnumerable();
    }
}
