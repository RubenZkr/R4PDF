namespace R4PDF.Models.Elements;

public class LineElement : PdfElement
{
    public string? X1 { get; set; }
    public string? Y1 { get; set; }
    public string? X2 { get; set; }
    public string? Y2 { get; set; }
    public string? StrokeWidth { get; set; }
    public string? Color { get; set; }
    public string? DashPattern { get; set; }
}
