using System.Collections.Generic;

namespace PRN232.Domain.ValueObjects;

public class HttpHeaderItem
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public HttpHeaderItem() { }
    public HttpHeaderItem(string name, string value)
    {
        Name = name;
        Value = value;
    }
}

public class VariableExtractor
{
    public string JsonPath { get; set; } = string.Empty;
    public string VariableName { get; set; } = string.Empty;

    public VariableExtractor() { }
    public VariableExtractor(string jsonPath, string variableName)
    {
        JsonPath = jsonPath;
        VariableName = variableName;
    }
}

public class ApiTestCase
{
    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = "GET"; // GET, POST, PUT, DELETE
    public string UrlPath { get; set; } = string.Empty;
    public string? RequestBody { get; set; } // JSON Body string
    public int ExpectedStatusCode { get; set; } = 200;
    public List<JsonAssertion> JsonAssertions { get; set; } = new();
    public List<HttpHeaderItem> Headers { get; set; } = new();
    public List<VariableExtractor> ExtractVariables { get; set; } = new();

    public ApiTestCase() { }

    public ApiTestCase(string name, string method, string urlPath, int expectedStatusCode, string? requestBody = null)
    {
        Name = name;
        Method = method;
        UrlPath = urlPath;
        ExpectedStatusCode = expectedStatusCode;
        RequestBody = requestBody;
    }
}
