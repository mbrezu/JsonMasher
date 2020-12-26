using System;
using System.Text;

namespace JsonMasher.Functions
{
    public class Times
    {
        public static Json Function(Json t1, Json t2)
            => (t1.Type, t2.Type) switch
            {
                (JsonValueType.Number, JsonValueType.Number)
                    => Json.Number(t1.GetNumber() * t2.GetNumber()),
                (JsonValueType.String, JsonValueType.Number)
                    => Json.String(Repeat(t1.GetString(), (int)t2.GetNumber())),
                (JsonValueType.Number, JsonValueType.String)
                    => Json.String(Repeat(t2.GetString(), (int)t1.GetNumber())),
                _ => throw new InvalidOperationException()
            };
        
        public static string Repeat(string str, int times)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < times; i++)
            {
                sb.Append(str);
            }
            return sb.ToString();
        }
    }
}
