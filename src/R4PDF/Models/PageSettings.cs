namespace R4PDF.Models;

public class PageSettings
{
    public string PageSize { get; set; } = PdfDefaults.PageSize;
    public string Orientation { get; set; } = PdfDefaults.Orientation;
    public MarginSettings Margins { get; set; } = new();
}