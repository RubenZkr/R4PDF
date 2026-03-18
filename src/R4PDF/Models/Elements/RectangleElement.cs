namespace R4PDF.Models.Elements;

public class RectangleElement : PdfElement
{
    public string? FillColor { get; set; }
    public string? StrokeColor { get; set; }
    public string? StrokeWidth { get; set; }
    public string? CornerRadius { get; set; }
}