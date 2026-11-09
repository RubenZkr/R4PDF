namespace R4PDF.Models.Elements;

public class TableElement : PdfElement
{
    public List<TableColumn> Columns { get; set; } = new();
    public List<TableRow> Rows { get; set; } = new();
    public PdfStyle? HeaderStyle { get; set; }
    public double? DataFontSize { get; set; }
    public bool ShowHeader { get; set; } = true;
    public bool AlternateRowColors { get; set; }
    public string? AlternateColor { get; set; }
    public BorderStyle? Borders { get; set; }
    public bool NoBorders { get; set; }

    /// <summary>
    ///     Where the content of a cell sits when its row is taller: "top" (default) or "middle".
    /// </summary>
    public string? VerticalAlignment { get; set; }

    public override PdfElement Clone()
    {
        return CloneWithRows(Rows.Select(row => row.Clone()).ToList());
    }

    /// <summary>A copy with other rows, for the parts of a table that is split over pages.</summary>
    public TableElement CloneWithRows(List<TableRow> rows)
    {
        var clone = (TableElement)base.Clone();
        clone.Columns = Columns.ToList();
        clone.Rows = rows;
        return clone;
    }
}
