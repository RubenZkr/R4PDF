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
}