namespace R4PDF.Models.Elements;

public class TableElement : PdfElement
{
    public List<TableColumn> Columns { get; set; } = new();
    public List<TableRow> Rows { get; set; } = new();
    public PdfStyle? HeaderStyle { get; set; }
    public bool ShowHeader { get; set; } = true;
    public bool AlternateRowColors { get; set; }
    public string? AlternateColor { get; set; }
    public BorderStyle? Borders { get; set; }
}

public class TableColumn
{
    public string Name { get; set; } = string.Empty;
    public string? Width { get; set; }
    public string? Alignment { get; set; }
}

public class TableRow
{
    public List<string> Cells { get; set; } = new();
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
}
