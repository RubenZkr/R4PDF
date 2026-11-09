using PdfSharpCore.Drawing;
using R4PDF.Models;
using R4PDF.Models.Elements;
using R4PDF.Parsing;

namespace R4PDF.Rendering;

public class TableRenderer
{
    public sealed class SplitRenderResult
    {
        public double HeightUsed { get; init; }
        public TableElement? RenderedPart { get; init; }
        public TableElement? Remainder { get; init; }
    }

    private const double DefaultCellPadding = PdfDefaults.TableCellPadding;

    /// <summary>Room between the text of a badge cell and the edge of its label.</summary>
    private const double BadgePaddingX = 4;

    private const double BadgePaddingY = 1.5;

    /// <summary>Records the anchors and links of the rows; set during the final render pass only.</summary>
    internal LayoutRecorder? Recorder { get; set; }

    public double Render(XGraphics gfx, TableElement table, ResolvedStyle style, double x, double y,
        double availableWidth)
    {
        var result = RenderWithContinuation(gfx, table, style, x, y, availableWidth, double.MaxValue, true);
        return result.HeightUsed;
    }

    /// <summary>
    ///     The height of the whole table and of its start (the header and the first row), without drawing
    ///     it; used to keep a heading together with the table that follows it.
    /// </summary>
    public (double Full, double Start) Measure(XGraphics gfx, TableElement table, ResolvedStyle style,
        double availableWidth)
    {
        if (table.Columns.Count == 0)
            return (0, 0);

        var columnWidths = CalculateColumnWidths(table, availableWidth);
        var headerHeight = table.ShowHeader ? MeasureHeaderHeight(gfx, table, columnWidths) : 0;
        var full = headerHeight + MeasureRowsHeight(gfx, table, columnWidths, 0, table.Rows.Count, style);
        var start = headerHeight + (table.Rows.Count > 0 ? MeasureDataRowHeight(gfx, table, table.Rows[0], columnWidths, style) : 0);
        return (full, start);
    }

    public SplitRenderResult RenderWithContinuation(XGraphics gfx, TableElement table, ResolvedStyle style,
        double x, double y, double availableWidth, double availableHeight, bool allowSplit)
    {
        if (table.Columns.Count == 0)
            return new SplitRenderResult { HeightUsed = 0, RenderedPart = (TableElement)table.Clone(), Remainder = null };

        var columnWidths = CalculateColumnWidths(table, availableWidth);
        var currentY = y;
        var borderPen = ResolveBorderPen(table.Borders, table.NoBorders);
        var heightUsed = 0d;
        var renderedRows = new List<TableRow>();
        var remainderRows = new List<TableRow>();

        var headerHeight = table.ShowHeader ? MeasureHeaderHeight(gfx, table, columnWidths) : 0;
        if (table.ShowHeader && headerHeight > availableHeight)
            return new SplitRenderResult { HeightUsed = 0, RenderedPart = null, Remainder = (TableElement)table.Clone() };

        if (!allowSplit)
        {
            var fullHeight = headerHeight + MeasureRowsHeight(gfx, table, columnWidths, 0, table.Rows.Count, style);
            if (fullHeight > availableHeight)
                return new SplitRenderResult { HeightUsed = 0, RenderedPart = null, Remainder = (TableElement)table.Clone() };
        }

        if (table.ShowHeader)
        {
            currentY += RenderHeaderRow(gfx, table, columnWidths, x, currentY, borderPen);
            heightUsed += headerHeight;
        }

        for (var i = 0; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];
            var rowHeight = MeasureDataRowHeight(gfx, table, row, columnWidths, style);
            var remainingHeight = availableHeight - heightUsed;

            if (rowHeight > remainingHeight)
            {
                remainderRows.Add(row.Clone());
                for (var j = i + 1; j < table.Rows.Count; j++)
                    remainderRows.Add(table.Rows[j].Clone());
                break;
            }

            XBrush? rowBackground = null;
            if (row.BackgroundColor != null)
                rowBackground = new XSolidBrush(ColorParser.Parse(row.BackgroundColor));
            else if (table.AlternateRowColors && i % 2 == 1)
                rowBackground =
                    new XSolidBrush(ColorParser.Parse(table.AlternateColor, XColor.FromArgb(245, 245, 245)));

            Recorder?.Record(row.Anchor, row.Link, new XRect(x, currentY, columnWidths.Sum(), rowHeight));
            currentY += RenderDataRow(gfx, table, row, columnWidths, x, currentY, borderPen, rowBackground, style);
            heightUsed += rowHeight;
            renderedRows.Add(row.Clone());
        }

        var renderedPart = renderedRows.Count > 0 || table.ShowHeader
            ? table.CloneWithRows(renderedRows)
            : null;

        var remainder = remainderRows.Count > 0
            ? table.CloneWithRows(remainderRows)
            : null;

        return new SplitRenderResult
        {
            HeightUsed = heightUsed,
            RenderedPart = renderedPart,
            Remainder = remainder
        };
    }

    private double[] CalculateColumnWidths(TableElement table, double availableWidth)
    {
        var widths = new double[table.Columns.Count];
        double totalFixed = 0;
        var autoCount = 0;

        for (var i = 0; i < table.Columns.Count; i++)
        {
            var colWidth = table.Columns[i].Width;
            if (colWidth != null && colWidth.EndsWith('%'))
            {
                var pct = double.Parse(colWidth.TrimEnd('%')) / 100.0;
                widths[i] = availableWidth * pct;
                totalFixed += widths[i];
            }
            else if (colWidth != null)
            {
                widths[i] = UnitConverter.ToPoints(colWidth);
                totalFixed += widths[i];
            }
            else
            {
                autoCount++;
            }
        }

        if (autoCount > 0)
        {
            var remaining = Math.Max(0, availableWidth - totalFixed) / autoCount;
            for (var i = 0; i < widths.Length; i++)
                if (widths[i] == 0)
                    widths[i] = remaining;
        }

        return widths;
    }

    private double MeasureHeaderHeight(XGraphics gfx, TableElement table, double[] columnWidths)
    {
        var headerFont = new XFont(FontFamilies.Helvetica, PdfDefaults.TableHeaderFontSize, XFontStyle.Bold);
        if (table.HeaderStyle != null)
        {
            var fontFamily = table.HeaderStyle.FontFamily ?? FontFamilies.Helvetica;
            var fontSize = table.HeaderStyle.FontSize ?? PdfDefaults.TableHeaderFontSize;
            var fontStyle = XFontStyle.Regular;
            if (table.HeaderStyle.FontWeight?.Equals(FontWeights.Bold, StringComparison.OrdinalIgnoreCase) == true)
                fontStyle |= XFontStyle.Bold;

            headerFont = new XFont(fontFamily, fontSize, fontStyle);
        }

        var lineHeight = headerFont.Height;
        var maxLines = 1;
        for (var i = 0; i < table.Columns.Count; i++)
        {
            var cellWidth = columnWidths[i] - DefaultCellPadding * 2;
            var wrapped = WrapCell(gfx, table.Columns[i].Name, headerFont, cellWidth);
            if (wrapped.Count > maxLines)
                maxLines = wrapped.Count;
        }

        return lineHeight * maxLines + DefaultCellPadding * 2;
    }

    private double MeasureRowsHeight(XGraphics gfx, TableElement table, double[] columnWidths, int start, int count,
        ResolvedStyle style)
    {
        var total = 0d;
        for (var i = start; i < start + count; i++)
            total += MeasureDataRowHeight(gfx, table, table.Rows[i], columnWidths, style);
        return total;
    }

    private double MeasureDataRowHeight(XGraphics gfx, TableElement table, TableRow row, double[] columnWidths,
        ResolvedStyle? style = null)
    {
        var cellPadding = GetDataCellPadding(table);
        var lineHeight = CellFont(table, row, null, style).Height;
        var cellCount = Math.Min(row.Cells.Count, table.Columns.Count);
        var maxLines = 1;

        for (var i = 0; i < cellCount; i++)
        {
            var wrapped = WrapCell(gfx, row.Cells[i], CellFont(table, row, CellStyleAt(row, i), style),
                TextWidth(columnWidths[i], cellPadding, CellStyleAt(row, i)));
            if (wrapped.Count > maxLines)
                maxLines = wrapped.Count;
        }

        return lineHeight * maxLines + cellPadding * 2;
    }

    private double RenderHeaderRow(XGraphics gfx, TableElement table, double[] columnWidths, double x, double y,
        XPen? borderPen)
    {
        var headerFont = new XFont(FontFamilies.Helvetica, PdfDefaults.TableHeaderFontSize, XFontStyle.Bold);
        var headerBrush = XBrushes.White;
        var headerBgColor = XColors.DarkGray;

        if (table.HeaderStyle != null)
        {
            var fontFamily = table.HeaderStyle.FontFamily ?? FontFamilies.Helvetica;
            var fontSize = table.HeaderStyle.FontSize ?? PdfDefaults.TableHeaderFontSize;
            var fontStyle = XFontStyle.Regular;
            if (table.HeaderStyle.FontWeight?.Equals(FontWeights.Bold, StringComparison.OrdinalIgnoreCase) == true)
                fontStyle |= XFontStyle.Bold;

            headerFont = new XFont(fontFamily, fontSize, fontStyle);
            headerBrush = new XSolidBrush(ColorParser.Parse(table.HeaderStyle.Color, XColors.White));
            headerBgColor = ColorParser.Parse(table.HeaderStyle.BackgroundColor, XColors.DarkGray);
        }

        var lineHeight = headerFont.Height;

        var wrappedHeaders = new List<string>[table.Columns.Count];
        var maxLines = 1;
        for (var i = 0; i < table.Columns.Count; i++)
        {
            var cellWidth = columnWidths[i] - DefaultCellPadding * 2;
            wrappedHeaders[i] = WrapCell(gfx, table.Columns[i].Name, headerFont, cellWidth);
            if (wrappedHeaders[i].Count > maxLines)
                maxLines = wrappedHeaders[i].Count;
        }

        var rowHeight = lineHeight * maxLines + DefaultCellPadding * 2;
        var currentX = x;

        gfx.DrawRectangle(new XSolidBrush(headerBgColor), x, y, columnWidths.Sum(), rowHeight);

        for (var i = 0; i < table.Columns.Count; i++)
        {
            var format = GetCellFormat(table.Columns[i].Alignment);
            var lines = wrappedHeaders[i];

            for (var li = 0; li < lines.Count; li++)
            {
                var lineRect = new XRect(currentX + DefaultCellPadding, y + DefaultCellPadding + li * lineHeight,
                    columnWidths[i] - DefaultCellPadding * 2, lineHeight);
                gfx.DrawString(lines[li], headerFont, headerBrush, lineRect, format);
            }

            if (borderPen != null)
                gfx.DrawRectangle(borderPen, currentX, y, columnWidths[i], rowHeight);

            currentX += columnWidths[i];
        }

        return rowHeight;
    }

    private double RenderDataRow(XGraphics gfx, TableElement table, TableRow row, double[] columnWidths,
        double x, double y, XPen? borderPen, XBrush? rowBackground, ResolvedStyle? style = null)
    {
        var cellPadding = GetDataCellPadding(table);
        var lineHeight = CellFont(table, row, null, style).Height;

        var cellCount = Math.Min(row.Cells.Count, table.Columns.Count);
        var wrappedCells = new List<string>[cellCount];
        var maxLines = 1;

        for (var i = 0; i < cellCount; i++)
        {
            wrappedCells[i] = WrapCell(gfx, row.Cells[i], CellFont(table, row, CellStyleAt(row, i), style),
                TextWidth(columnWidths[i], cellPadding, CellStyleAt(row, i)));
            if (wrappedCells[i].Count > maxLines)
                maxLines = wrappedCells[i].Count;
        }

        var rowHeight = lineHeight * maxLines + cellPadding * 2;
        var currentX = x;

        if (rowBackground != null)
            gfx.DrawRectangle(rowBackground, x, y, columnWidths.Sum(), rowHeight);

        for (var i = 0; i < cellCount; i++)
        {
            var cellStyle = CellStyleAt(row, i);
            var font = CellFont(table, row, cellStyle, style);
            var textBrush = new XSolidBrush(ColorParser.Parse(cellStyle?.TextColor ?? row.TextColor, XColors.Black));
            var alignment = i < table.Columns.Count ? table.Columns[i].Alignment : null;
            var format = GetCellFormat(alignment);
            var lines = wrappedCells[i];
            var inset = cellStyle?.Badge == true ? BadgePaddingX : 0;
            var textWidth = TextWidth(columnWidths[i], cellPadding, cellStyle);

            if (cellStyle?.BackgroundColor != null)
            {
                var background = new XSolidBrush(ColorParser.Parse(cellStyle.BackgroundColor));
                if (cellStyle.Badge)
                    DrawBadge(gfx, background, lines, font, format, currentX + cellPadding, y + cellPadding,
                        columnWidths[i] - cellPadding * 2, lineHeight);
                else
                    gfx.DrawRectangle(background, currentX, y, columnWidths[i], rowHeight);
            }

            for (var li = 0; li < lines.Count; li++)
            {
                var lineRect = new XRect(currentX + cellPadding + inset, y + cellPadding + li * lineHeight,
                    textWidth, lineHeight);
                gfx.DrawString(lines[li], font, textBrush, lineRect, format);
            }

            if (borderPen != null)
                gfx.DrawRectangle(borderPen, currentX, y, columnWidths[i], rowHeight);

            currentX += columnWidths[i];
        }

        return rowHeight;
    }

    /// <summary>
    ///     A rounded label as wide as the longest line plus its padding, placed like the text (left, centred
    ///     or right) within the content area of the cell.
    /// </summary>
    private static void DrawBadge(XGraphics gfx, XBrush brush, IReadOnlyList<string> lines, XFont font,
        XStringFormat format, double left, double top, double width, double lineHeight)
    {
        if (lines.All(string.IsNullOrEmpty))
            return;

        var badgeWidth = Math.Min(width, lines.Max(line => gfx.MeasureString(line, font).Width) + BadgePaddingX * 2);
        var badgeHeight = lines.Count * lineHeight + BadgePaddingY * 2;
        var badgeX = format.Alignment switch
        {
            XStringAlignment.Center => left + (width - badgeWidth) / 2,
            XStringAlignment.Far => left + width - badgeWidth,
            _ => left
        };

        var corner = Math.Min(badgeHeight, lineHeight + BadgePaddingY * 2);
        gfx.DrawRoundedRectangle(brush, badgeX, top - BadgePaddingY, badgeWidth, badgeHeight, corner, corner);
    }

    private static TableCellStyle? CellStyleAt(TableRow row, int index)
    {
        return row.CellStyles != null && index < row.CellStyles.Count ? row.CellStyles[index] : null;
    }

    /// <summary>The font of a cell: bold when its style says so, otherwise when its row is bold.</summary>
    private static XFont CellFont(TableElement table, TableRow row, TableCellStyle? cellStyle, ResolvedStyle? style)
    {
        var bold = cellStyle?.IsBold ?? row.IsBold;
        return new XFont(style?.FontFamily ?? FontFamilies.Helvetica, table.DataFontSize ?? PdfDefaults.TableDataFontSize,
            bold ? XFontStyle.Bold : XFontStyle.Regular);
    }

    /// <summary>The width the text of a cell wraps in; a badge keeps room for its padding.</summary>
    private static double TextWidth(double columnWidth, double cellPadding, TableCellStyle? cellStyle)
    {
        var inset = cellStyle?.Badge == true ? BadgePaddingX * 2 : 0;
        return Math.Max(1, columnWidth - cellPadding * 2 - inset);
    }

    /// <summary>A cell always has at least one line, so an empty cell still has the height of a row.</summary>
    private static List<string> WrapCell(XGraphics gfx, string? text, XFont font, double maxWidth)
    {
        var lines = TextWrapper.Wrap(gfx, text, font, maxWidth);
        if (lines.Count == 0)
            lines.Add(string.Empty);
        return lines;
    }

    private static double GetDataCellPadding(TableElement table)
    {
        var dataFontSize = table.DataFontSize ?? PdfDefaults.TableDataFontSize;
        var scale = dataFontSize / PdfDefaults.TableDataFontSize;

        return Math.Max(1d, DefaultCellPadding * scale);
    }

    private static XPen? ResolveBorderPen(BorderStyle? borders, bool noBorders = false)
    {
        if (noBorders)
            return null;

        if (borders == null)
            return new XPen(XColors.LightGray, PdfDefaults.BorderWidth);

        var color = ColorParser.Parse(borders.Color, XColors.LightGray);
        var width = UnitConverter.ToPoints(borders.Width, PdfDefaults.BorderWidth);
        return new XPen(color, width);
    }

    private static XStringFormat GetCellFormat(string? alignment)
    {
        var format = new XStringFormat { LineAlignment = XLineAlignment.Center };
        format.Alignment = alignment?.ToLowerInvariant() switch
        {
            Alignments.Center => XStringAlignment.Center,
            Alignments.Right => XStringAlignment.Far,
            _ => XStringAlignment.Near
        };
        return format;
    }
}
