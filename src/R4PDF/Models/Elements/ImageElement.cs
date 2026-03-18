namespace R4PDF.Models.Elements;

public class ImageElement : PdfElement
{
    public string Source { get; set; } = string.Empty;
    public string? Alignment { get; set; }
    public bool MaintainAspectRatio { get; set; } = true;
}