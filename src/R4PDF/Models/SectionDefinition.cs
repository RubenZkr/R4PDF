using R4PDF.Models.Elements;

namespace R4PDF.Models;

public class SectionDefinition
{
    public string? Height { get; set; }
    public string? Background { get; set; }
    public List<PdfElement> Elements { get; set; } = new();
}