using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JsonMasher.Mashers
{
    public interface PathPart {};
    public record StringPathPart(string Value): PathPart;
    public record IntPathPart(int Value): PathPart;
    public record SlicePathPart(int Start, int End): PathPart;
    public class Path
    {
        ImmutableList<PathPart> _parts;

        public IEnumerable<PathPart> Parts => _parts;

        public Path(IEnumerable<PathPart> parts) => _parts = FromParts(parts);

        public Path(params PathPart[] parts) => _parts = FromParts(parts);

        private Path(ImmutableList<PathPart> parts) => _parts = parts;

        private static ImmutableList<PathPart> FromParts(IEnumerable<PathPart> parts)
            => ImmutableList<PathPart>.Empty.AddRange(parts);
        
        public Path Extend(PathPart part) => new Path(_parts.Add(part));

        public Json ToJsonArray() => Json.Array(_parts.Select(p => p switch {
            StringPathPart sp => Json.String(sp.Value),
            IntPathPart ip => Json.Number(ip.Value),
            SlicePathPart slicePathPart => Json.ObjectParams(
                new JsonProperty("start", Json.Number(slicePathPart.Start)),
                new JsonProperty("end", Json.Number(slicePathPart.End))),
            _ => throw new InvalidOperationException()
        }));

        private static Path _empty = new Path();
        public static Path Empty = _empty;
    }

    public record PathAndValue(Path Path, Json Value);

    public interface IPathGenerator
    {
        IEnumerable<PathAndValue> GeneratePaths(
            Path pathSoFar, Json json, IMashContext context, IMashStack stack);
    }
}
