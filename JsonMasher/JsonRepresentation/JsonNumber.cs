namespace JsonMasher.JsonRepresentation
{
    class JsonNumber : Json
    {
        double _value;

        public JsonNumber(double value)
        {
            _value = value;
            Type = JsonValueType.Number;
        }

        public override double GetNumber()
            => _value;
    }
}
