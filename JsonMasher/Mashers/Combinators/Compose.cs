using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonMasher.Mashers.Combinators
{
    public class Compose : IJsonMasherOperator, IJsonZipper
    {
        public IJsonMasherOperator First { get; init; }
        public IJsonMasherOperator Second { get; init; }

        public IEnumerable<Json> Mash(Json json, IMashContext context, IMashStack stack)
        {
            var newStack = stack.Push(this);
            context.Tick(stack);
            foreach (var temp in First.Mash(json, context, newStack))
            {
                foreach (var result in Second.Mash(temp, context, newStack))
                {
                    yield return result;
                }
            }
        }

        public static IJsonMasherOperator AllParams(params IJsonMasherOperator[] mashers)
            => All(mashers);

        public static IJsonMasherOperator All(IEnumerable<IJsonMasherOperator> mashers)
            => mashers.Fold((m1, m2) => new Compose { First = m1, Second = m2 });

        public ZipStage ZipDown(Json json, IMashContext context, IMashStack stack)
        {
            if (First is IJsonZipper zipper1)
            {
                if (Second is IJsonZipper zipper2)
                {
                    context.Tick(stack);
                    var zipStage1 = zipper1.ZipDown(json, context, stack);
                    var zipStages2 = zipStage1.Parts.Select(
                        part => zipper2.ZipDown(part, context, stack));
                    return new ZipStage(
                        parts => Reconstruct(zipStage1, zipStages2, parts),
                        zipStages2.SelectMany(stage => stage.Parts));
                }
            }
            throw new InvalidOperationException();
        }

        private Json Reconstruct(
            ZipStage zipStage1, IEnumerable<ZipStage> zipStages2, IEnumerable<Json> parts)
            => zipStage1.ZipUp(Reconstruct(zipStages2, parts));

        private IEnumerable<Json> Reconstruct(
            IEnumerable<ZipStage> zipStages, IEnumerable<Json> parts)
        {
            foreach (var stage in zipStages)
            {
                var stageSize = stage.Parts.Count();
                var partsForStage = parts.Take(stageSize);
                yield return stage.ZipUp(partsForStage);
                parts = parts.Skip(stageSize);
            }
        }
    }
}
