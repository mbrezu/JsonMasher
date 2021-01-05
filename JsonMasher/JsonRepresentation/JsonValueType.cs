namespace JsonMasher.JsonRepresentation
{
    // Order is important, as it is the primary sort key
    // (see https://stedolan.github.io/jq/manual/#Builtinoperatorsandfunctions,
    // search for 'sort, sort_by').
    public enum JsonValueType : byte
    {
        Undefined,
        Null,
        False,
        True,
        Number,
        String,
        Array,
        Object,
    }
}
