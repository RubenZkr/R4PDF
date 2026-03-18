namespace R4PDF.Models;

public class PageSettings
{
    public string PageSize { get; set; } = PdfDefaults.PageSize;
    public string Orientation { get; set; } = PdfDefaults.Orientation;
    public MarginSettings Margins { get; set; } = new();
}

public class MarginSettings
{
    public string Top { get; set; } = PdfDefaults.MarginTop;
    public string Bottom { get; set; } = PdfDefaults.MarginBottom;
    public string Left { get; set; } = PdfDefaults.MarginLeft;
    public string Right { get; set; } = PdfDefaults.MarginRight;
}
