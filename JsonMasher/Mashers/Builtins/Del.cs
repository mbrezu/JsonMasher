using System.Collections.Generic;
using JsonMasher.Mashers.Combinators;
using System.Linq;
using System;
using JsonMasher.JsonRepresentation;

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
                    (JsonValueType.Array, SlicePathPart spp) => json.DelSliceAt(spp.Start, spp.End),
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
                        => json.GetSliceAt(spp.Start, spp.End),
                    (JsonValueType.Object, StringPathPart sp) => json.GetElementAt(sp.Value),
                    _ => throw IndexingError(json, path, context, stack)
                };
                var updatedComponent = DeletePath(component, restOfPath, context, stack);
                return (json.Type, key) switch {
                    (JsonValueType.Array, IntPathPart ip) => json.SetElementAt(ip.Value, updatedComponent),
                    (JsonValueType.Array, SlicePathPart spp)
                        => json.SetSliceAt(spp.Start, spp.End, updatedComponent),
                    (JsonValueType.Object, StringPathPart sp) => json.SetElementAt(sp.Value, updatedComponent),
                    _ => throw IndexingError(json, path, context, stack)
                };
            }
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
