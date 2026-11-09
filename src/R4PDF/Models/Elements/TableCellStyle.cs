namespace R4PDF.Models.Elements;

/// <summary>
///     The style of a single cell, over the style of its row: a status in its own color, for example. A
///     property that is not set keeps the value of the row.
/// </summary>
public class TableCellStyle
{
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
    public bool? IsBold { get; set; }

    /// <summary>
    ///     Draws the background as a rounded label around the text instead of filling the cell, so a
    ///     status reads as a badge.
    /// </summary>
    public bool Badge { get; set; }
}
