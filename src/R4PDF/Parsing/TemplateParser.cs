using System.Text.Json;
using System.Text.Json.Serialization;
using R4PDF.Models;

namespace R4PDF.Parsing;

/// <summary>
/// Deserializes a JSON template string into a PdfTemplate model.
/// </summary>
public static class TemplateParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static PdfTemplate Parse(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var template = JsonSerializer.Deserialize<PdfTemplate>(json, Options)
            ?? throw new JsonException("Failed to deserialize PDF template: result was null.");

        Validate(template);
        return template;
    }

    private static void Validate(PdfTemplate template)
    {
        if (template.Pages.Count == 0)
            throw new JsonException("PDF template must have at least one page.");

        foreach (var page in template.Pages)
        {
            if (page.Body.Elements.Count == 0
                && (page.Header?.Elements.Count ?? 0) == 0
                && (page.Footer?.Elements.Count ?? 0) == 0)
            {
                throw new JsonException("Each page must have at least one element in header, body, or footer.");
            }
        }
    }
}
