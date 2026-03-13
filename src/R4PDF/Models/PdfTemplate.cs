namespace R4PDF.Models;

/// <summary>
/// Root model for a PDF template. Defines the document structure, pages, and reusable styles.
/// </summary>
public class PdfTemplate
{
    public DocumentMetadata? Metadata { get; set; }
    public PageSettings Settings { get; set; } = new();
    public Dictionary<string, PdfStyle> Styles { get; set; } = new();
    public List<PageDefinition> Pages { get; set; } = new();
}

public class DocumentMetadata
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Subject { get; set; }
    public string? Keywords { get; set; }
}

public class PageDefinition
{
    public PageSettings? Settings { get; set; }
    public SectionDefinition? Header { get; set; }
    public SectionDefinition Body { get; set; } = new();
    public SectionDefinition? Footer { get; set; }
}

public class SectionDefinition
{
    public string? Height { get; set; }
    public string? Background { get; set; }
    public List<Elements.PdfElement> Elements { get; set; } = new();
}
