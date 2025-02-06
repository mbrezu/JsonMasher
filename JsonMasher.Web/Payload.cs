namespace JsonMasher.Web;

public class Payload
{
    public string Program { get; set; } = "";
    public string Input { get; set; } = "";
    public string? Slurp { get; set; } = "";

    public string ExampleProgram { get; set; } = "";

    public bool IsSlurping => Slurp?.ToLower()?.Trim() == "on";
}
