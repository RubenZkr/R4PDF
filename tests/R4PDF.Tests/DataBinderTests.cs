using R4PDF.Parsing;

namespace R4PDF.Tests;

public class DataBinderTests
{
    [Fact]
    public void Bind_SimplePlaceholders_ResolvesValues()
    {
        var template = """{"text": "${name}", "value": "${age}"}""";
        var data = """{"name": "John", "age": 30}""";

        var result = DataBinder.Bind(template, data);

        Assert.Contains("John", result);
        Assert.Contains("30", result);
    }

    [Fact]
    public void Bind_NestedPaths_ResolvesValues()
    {
        var template = """{"text": "${person.name}", "city": "${person.address.city}"}""";
        var data = """{"person": {"name": "Jane", "address": {"city": "NYC"}}}""";

        var result = DataBinder.Bind(template, data);

        Assert.Contains("Jane", result);
        Assert.Contains("NYC", result);
    }

    [Fact]
    public void Bind_MissingKey_LeavesPlaceholderAsIs()
    {
        var template = """{"text": "${missing}"}""";
        var data = """{"name": "John"}""";

        var result = DataBinder.Bind(template, data);

        Assert.Contains("${missing}", result);
    }

    [Fact]
    public void Bind_NullData_ReturnsTemplateUnchanged()
    {
        var template = """{"text": "${name}"}""";
        var result = DataBinder.Bind(template, null);

        Assert.Equal(template, result);
    }

    [Fact]
    public void Bind_BooleanValue_ResolvesCorrectly()
    {
        var template = """{"active": "${isActive}"}""";
        var data = """{"isActive": true}""";

        var result = DataBinder.Bind(template, data);

        Assert.Contains("true", result);
    }
}
