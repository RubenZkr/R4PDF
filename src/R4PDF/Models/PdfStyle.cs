namespace R4PDF.Models;

public class PdfStyle
{
    public string? FontFamily { get; set; }
    public double? FontSize { get; set; }
    public string? FontWeight { get; set; }
    public string? FontStyle { get; set; }
    public string? Color { get; set; }
    public string? BackgroundColor { get; set; }
    public string? Alignment { get; set; }
    public double? LineHeight { get; set; }
    public string? Padding { get; set; }
    public BorderStyle? Border { get; set; }
}

public class BorderStyle
{
    public string? Width { get; set; }
    public string? Color { get; set; }
    public string Type { get; set; } = "all";
}
