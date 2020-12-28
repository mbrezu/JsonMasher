using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Combinators
{
    public class PipeAssignment : IJsonMasherOperator
    {
        public IJsonMasherOperator PathExpression { get; init; }
        public IJsonMasherOperator Masher { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context)
        {
            if (PathExpression is IJsonZipper zipper)
            {
                var zipStage = zipper.ZipDown(json, context);
                var results = zipStage.Parts.Select(p => Masher.Mash(p, context).First());
                yield return zipStage.ZipUp(results);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
