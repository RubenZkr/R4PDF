namespace R4PDF.Models.Elements;

public class TableRow
{
    public List<string> Cells { get; set; } = new();
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
}