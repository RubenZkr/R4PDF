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