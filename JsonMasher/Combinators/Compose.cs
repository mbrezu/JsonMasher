using System.Collections.Generic;
using System.Linq;
using JsonMasher.Primitives;

namespace JsonMasher.Combinators
{
    public class Compose : IJsonMasher
    {
        public IJsonMasher First { get; init; }
        public IJsonMasher Second { get; init; }

        public IEnumerable<Json> Mash(IEnumerable<Json> seq)
        {
            foreach (var json in seq)
            {
                foreach (var result in Mash(json))
                {
                    yield return result;
                }
            }
        }

        public IEnumerable<Json> Mash(Json json)
        {
            foreach (var temp in First.Mash(json))
            {
                foreach (var result in Second.Mash(temp))
                {
                    yield return result;
                }
            }
        }

        public static IJsonMasher AllParams(params IJsonMasher[] mashers)
            => All(mashers);

        public static IJsonMasher All(IEnumerable<IJsonMasher> mashers)
            => mashers.Count() switch
            {
                0 => Identity.Instance,
                1 => mashers.First(),
                _ => new Compose 
                { 
                    First = mashers.First(), 
                    Second = All(mashers.Skip(1))
                }
            };
    }
}
