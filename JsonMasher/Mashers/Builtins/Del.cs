using System.Collections.Generic;
using JsonMasher.Mashers.Combinators;
using System.Linq;
using System;

namespace JsonMasher.Mashers.Builtins
{
    public class Del
    {
        private static Builtin _builtin = new Builtin(Function, 1);

        private static IEnumerable<Json> Function(
            List<IJsonMasherOperator> mashers, Json json, IMashContext context, IMashStack stack)
        {
            if (mashers[0] is IPathGenerator pathGenerator)
            {
                var paths = pathGenerator
                    .GeneratePaths(JsonPath.Empty, json, context, stack)
                    .Select(pv => new { Key = pv.Path.ToJsonArray(), Path = pv.Path })
                    .OrderByDescending(x => x.Key, JsonComparer.Instance)
                    .Select(x => x.Path);
                foreach (var path in paths)
                {
                    json = DeletePath(json, path, context, stack);
                }
                yield return json;
            }
            else
            {
                throw context.Error("Not a path expression.", stack.Push(mashers[0]));
            }
        }

        private static Json DeletePath(Json json, JsonPath path, IMashContext context, IMashStack stack)
        {
            if (path == JsonPath.Empty)
            {
                return Json.Null;
            }
            else if (path.Parts.Count() == 1)
            {
                return (json.Type, path.Parts.First()) switch {
                    (JsonValueType.Array, IntPathPart ip) => json.DelElementAt(ip.Value),
                    (JsonValueType.Array, SlicePathPart spp) => DeleteSlice(json, spp.Start, spp.End),
                    (JsonValueType.Object, StringPathPart sp) => json.DelElementAt(sp.Value),
                    _ => throw IndexingError(json, path, context, stack)
                };
            }
            else
            {
                var key = path.Parts.First();
                var restOfPath = path.WithoutFirstPart;
                var component = (json.Type, key) switch {
                    (JsonValueType.Array, IntPathPart ip) => json.GetElementAt(ip.Value),
                    (JsonValueType.Array, SlicePathPart spp)
                        => SliceSelector.GetSlice(json, Tuple.Create(spp.Start, spp.End)),
                    (JsonValueType.Object, StringPathPart sp) => json.GetElementAt(sp.Value),
                    _ => throw IndexingError(json, path, context, stack)
                };
                var updatedComponent = DeletePath(component, restOfPath, context, stack);
                return (json.Type, key) switch {
                    (JsonValueType.Array, IntPathPart ip) => json.SetElementAt(ip.Value, updatedComponent),
                    (JsonValueType.Array, SlicePathPart spp)
                        => Json.Array(json.EnumerateArray().Take(spp.Start)
                            .Concat(updatedComponent.EnumerateArray())
                            .Concat(json.EnumerateArray().Skip(spp.End))),
                    (JsonValueType.Object, StringPathPart sp) => json.SetElementAt(sp.Value, updatedComponent),
                    _ => throw IndexingError(json, path, context, stack)
                };
            }
        }

        private static Json DeleteSlice(Json json, int start, int end)
        {
            for (int i = end - 1; i >= start; i--)
            {
                json = json.DelElementAt(i);
            }
            return json;
        }

        private static Exception IndexingError(
            Json json, JsonPath path, IMashContext context, IMashStack stack)
        {
            var key = path.ToJsonArray().EnumerateArray().First();
            throw context.Error(
                $"Can't index {json.Type} with {key.Type}.", stack, json, key);
        }

        public static Builtin Builtin => _builtin;
    }
}
