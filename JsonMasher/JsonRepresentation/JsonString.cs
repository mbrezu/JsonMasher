namespace JsonMasher.JsonRepresentation
{
    class JsonString : Json
    {
        string _value;

        public JsonString(string value)
        {
            _value = value;
            Type = JsonValueType.String;
        }

        public override string GetString() => _value;

        public override int GetLength() => _value.Length;
    }
}
