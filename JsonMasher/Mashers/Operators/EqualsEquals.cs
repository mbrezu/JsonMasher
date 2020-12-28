namespace JsonMasher.Mashers.Operators
{
    public class EqualsEquals
    {
        public static Json Operator(Json t1, Json t2)
            => t1.DeepEqual(t2) ? Json.True : Json.False;
    }
}
