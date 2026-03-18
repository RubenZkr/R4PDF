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
