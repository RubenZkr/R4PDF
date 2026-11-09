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

    /// <summary>The font size of the cell, over the data font size of the table.</summary>
    public double? FontSize { get; set; }

    /// <summary>
    ///     Draws the background as a rounded label around the text instead of filling the cell, so a
    ///     status reads as a badge.
    /// </summary>
    public bool Badge { get; set; }

    /// <summary>A small dot of this color before the text of a <see cref="Badge" />.</summary>
    public string? DotColor { get; set; }

    /// <summary>
    ///     The content of the cell as pieces of text with their own size, weight and color, instead of
    ///     the cell text: a bold name followed by a muted date, for example. A newline in a run starts a
    ///     new line; spaces at the start of a run are kept, so they can space out the pieces.
    /// </summary>
    public List<TextRun>? Runs { get; set; }

    /// <summary>Fills the rest of the last line with a dotted leader, as a table of contents does before its page number.</summary>
    public bool Leader { get; set; }

    public string? LeaderColor { get; set; }

    /// <summary>A line along the bottom of the cell, to separate rows.</summary>
    public string? BorderBottomColor { get; set; }

    public string? BorderBottomWidth { get; set; }

    /// <summary>Moves the content of the cell to the right, as a tree indents its children.</summary>
    public string? Indent { get; set; }
}

/// <summary>A piece of text in a cell with its own style; see <see cref="TableCellStyle.Runs" />.</summary>
public class TextRun
{
    public string Text { get; set; } = string.Empty;
    public double? FontSize { get; set; }
    public bool? IsBold { get; set; }
    public string? Color { get; set; }

    /// <summary>
    ///     Draws the run as a chip: a rounded label of this color behind its text, kept on one line. Spaces
    ///     before the text stay outside the chip.
    /// </summary>
    public string? BackgroundColor { get; set; }

    /// <summary>Names the place of the run; see <see cref="PdfElement.Anchor" />. Each word of the run reports its area.</summary>
    public string? Anchor { get; set; }

    /// <summary>Makes the run clickable; see <see cref="PdfElement.Link" />.</summary>
    public string? Link { get; set; }
}
