namespace R4PDF.Models;

public class BorderStyle
{
    public string? Width { get; set; }
    public string? Color { get; set; }
    public string Type { get; set; } = PdfDefaults.BorderType;
}