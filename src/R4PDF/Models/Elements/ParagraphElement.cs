namespace R4PDF.Models.Elements;

public class ParagraphElement : PdfElement
{
    public string Content { get; set; } = string.Empty;
    public string? FontFamily { get; set; }
    public double? FontSize { get; set; }
    public string? FontWeight { get; set; }
    public string? Color { get; set; }
    public string? Alignment { get; set; }
    public double? LineHeight { get; set; }
    public SpacingSettings? Spacing { get; set; }

    /// <summary>A copy with other content, for the parts of a paragraph that is split over pages.</summary>
    public ParagraphElement CloneWithContent(string content)
    {
        var clone = (ParagraphElement)Clone();
        clone.Content = content;
        return clone;
    }
}
