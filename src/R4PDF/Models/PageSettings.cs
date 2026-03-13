namespace R4PDF.Models;

public class PageSettings
{
    public string PageSize { get; set; } = "A4";
    public string Orientation { get; set; } = "Portrait";
    public MarginSettings Margins { get; set; } = new();
}

public class MarginSettings
{
    public string Top { get; set; } = "20mm";
    public string Bottom { get; set; } = "20mm";
    public string Left { get; set; } = "15mm";
    public string Right { get; set; } = "15mm";
}
