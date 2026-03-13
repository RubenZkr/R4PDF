namespace R4PDF.Models.Elements;

public class TextElement : PdfElement
{
    public string Text { get; set; } = string.Empty;
    public string? FontFamily { get; set; }
    public double? FontSize { get; set; }
    public string? FontWeight { get; set; }
    public string? Color { get; set; }
    public string? Alignment { get; set; }
}
