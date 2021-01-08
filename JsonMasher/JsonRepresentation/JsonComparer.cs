using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace JsonMasher.JsonRepresentation
{
    public class JsonComparer : Comparer<Json>, IEqualityComparer<Json>
    {
        public override int Compare(Json x, Json y)
        {
            if (x.Type < y.Type)
            {
                return -1;
            }
            else if (x.Type > y.Type)
            {
                return 1;
            }
            else
            {
                return x.Type switch
                {
                    JsonValueType.True
                        or JsonValueType.False
                        or JsonValueType.Null
                        or JsonValueType.Undefined => 0,
                    JsonValueType.Number => x.GetNumber().CompareTo(y.GetNumber()),
                    JsonValueType.String => x.GetString().CompareTo(y.GetString()),
                    JsonValueType.Array => CompareArrays(x, y),
                    JsonValueType.Object => CompareObjects(x, y),
                    _ => throw new NotImplementedException()
                };
            }
        }

        private int CompareArrays(Json x, Json y)
        {
            int i = 0;
            while (true)
            {
                bool xAtEnd = i == x.GetLength();
                bool yAtEnd = i == y.GetLength();
                if (xAtEnd && yAtEnd)
                {
                    return 0;
                }
                if (xAtEnd && !yAtEnd)
                {
                    return -1;
                }
                if (!xAtEnd && yAtEnd)
                {
                    return 1;
                }
                var elementCompare = Compare(x.GetElementAt(i), y.GetElementAt(i));
                if (elementCompare != 0)
                {
                    return elementCompare;
                }
                i++;
            }
        }

        private int CompareObjects(Json x, Json y)
        {
            var xkv = x.EnumerateObject();
            var xk = xkv.Select(kv => Json.String(kv.Key));

            var ykv = y.EnumerateObject();
            var yk = ykv.Select(kv => Json.String(kv.Key));

            int keysCompare = Compare(Json.Array(xk), Json.Array(yk));
            if (keysCompare != 0)
            {
                return keysCompare;
            }

            var xv = xkv.Select(kv => kv.Value);
            var yv = ykv.Select(kv => kv.Value);
            return Compare(Json.Array(xv), Json.Array(yv));
        }

        public bool Equals(Json x, Json y) => x != null && x.DeepEqual(y);

        public int GetHashCode([DisallowNull] Json obj) => obj.ToString().GetHashCode();

        private JsonComparer()
        {
        }

        private static JsonComparer _instance = new JsonComparer();
        public static JsonComparer Instance = _instance;
    }
}
