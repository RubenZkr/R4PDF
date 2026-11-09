namespace R4PDF.Models.Elements;

public class TableRow
{
    public List<string> Cells { get; set; } = new();
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
    public bool IsBold { get; set; }

    /// <summary>The style of each cell, in column order; a missing or null entry keeps the style of the row.</summary>
    public List<TableCellStyle?>? CellStyles { get; set; }

    /// <summary>Names the row; see <see cref="PdfElement.Anchor" />.</summary>
    public string? Anchor { get; set; }

    /// <summary>Makes the whole row clickable; see <see cref="PdfElement.Link" />.</summary>
    public string? Link { get; set; }

    public TableRow Clone()
    {
        var clone = (TableRow)MemberwiseClone();
        clone.Cells = Cells.ToList();
        clone.CellStyles = CellStyles?.ToList();
        return clone;
    }
}
