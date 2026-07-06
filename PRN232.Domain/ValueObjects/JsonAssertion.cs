namespace PRN232.Domain.ValueObjects;

public class JsonAssertion
{
    public string JsonPath { get; set; } = string.Empty;
    public string Operator { get; set; } = "Equal"; // Equal, NotNull, Contains, Empty
    public string ExpectedValue { get; set; } = string.Empty;

    public JsonAssertion() { }

    public JsonAssertion(string jsonPath, string op, string expectedValue)
    {
        JsonPath = jsonPath;
        Operator = op;
        ExpectedValue = expectedValue;
    }
}
