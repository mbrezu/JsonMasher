using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Combinators
{
    public class PipeAssignment : IJsonMasherOperator
    {
        public IJsonMasherOperator PathExpression { get; init; }
        public IJsonMasherOperator Masher { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(stack);
            if (PathExpression is IJsonZipper zipper)
            {
                var zipStage = zipper.ZipDown(json, context, newStack);
                var results = zipStage.Parts.Select(p => Masher.Mash(p, context, newStack).First());
                yield return zipStage.ZipUp(results);
            }
            else
            {
                throw context.Error($"Not a path expression.", newStack.Push(PathExpression));
            }
        }
    }
}
