using System.Text.Json;
using System.Text.RegularExpressions;

namespace R4PDF.Parsing;

/// <summary>
///     Resolves ${path.to.value} placeholders in a JSON template string using values from a data JSON object.
/// </summary>
public static partial class DataBinder
{
    [GeneratedRegex(@"\$\{([^}]+)\}")]
    private static partial Regex PlaceholderRegex();

    public static string Bind(string templateJson, string? dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson))
            return templateJson;

        using var doc = JsonDocument.Parse(dataJson);
        var root = doc.RootElement;

        return PlaceholderRegex().Replace(templateJson, match =>
        {
            var path = match.Groups[1].Value;
            var value = ResolvePath(root, path);
            return value ?? match.Value; // leave unresolved placeholders as-is
        });
    }

    private static string? ResolvePath(JsonElement element, string path)
    {
        var segments = path.Split('.');
        var current = element;

        foreach (var segment in segments)
            if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(segment, out var next))
                current = next;
            else
                return null;

        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString(),
            JsonValueKind.Number => current.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "",
            _ => current.GetRawText()
        };
    }
}