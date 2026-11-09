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
        var rowLineHeight = CellFont(table, row, null, style).Height;
        var cellCount = Math.Min(row.Cells.Count, table.Columns.Count);
        var contentHeight = (double)rowLineHeight;

        for (var i = 0; i < cellCount; i++)
            contentHeight = Math.Max(contentHeight, LayoutCell(gfx, table, row, i, columnWidths[i], cellPadding, style).Height);

        return contentHeight + cellPadding * 2;
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
        var cellCount = Math.Min(row.Cells.Count, table.Columns.Count);
        var cells = Enumerable.Range(0, cellCount)
            .Select(i => LayoutCell(gfx, table, row, i, columnWidths[i], cellPadding, style))
            .ToList();

        var contentHeight = cells.Select(cell => cell.Height).Append((double)CellFont(table, row, null, style).Height).Max();
        var rowHeight = contentHeight + cellPadding * 2;
        var middle = string.Equals(table.VerticalAlignment, "middle", StringComparison.OrdinalIgnoreCase);
        var currentX = x;

        if (rowBackground != null)
            gfx.DrawRectangle(rowBackground, x, y, columnWidths.Sum(), rowHeight);

        for (var i = 0; i < cellCount; i++)
        {
            var cell = cells[i];
            var cellStyle = CellStyleAt(row, i);
            var alignment = i < table.Columns.Count ? table.Columns[i].Alignment : null;
            var format = GetCellFormat(alignment);
            var top = y + cellPadding + (middle ? (contentHeight - cell.Height) / 2 : 0);
            var inset = BadgeInset(cellStyle);

            if (cellStyle?.BackgroundColor != null)
            {
                var background = new XSolidBrush(ColorParser.Parse(cellStyle.BackgroundColor));
                if (cellStyle.Badge)
                    DrawBadge(gfx, background, cellStyle.DotColor, cell, format, currentX + cellPadding, top,
                        columnWidths[i] - cellPadding * 2);
                else
                    gfx.DrawRectangle(background, currentX, y, columnWidths[i], rowHeight);
            }

            var lineTop = top;
            var lastLineEnd = currentX + cellPadding + inset;
            for (var li = 0; li < cell.Lines.Count; li++)
            {
                var line = cell.Lines[li];
                var lineHeight = cell.LineHeights[li];
                var left = currentX + cellPadding + inset;

                if (cell.IsRuns)
                {
                    var lineWidth = line.Sum(segment => segment.Width);
                    var lineX = format.Alignment switch
                    {
                        XStringAlignment.Center => left + (cell.TextWidth - lineWidth) / 2,
                        XStringAlignment.Far => left + cell.TextWidth - lineWidth,
                        _ => left
                    };
                    var baseline = lineTop + line.Max(segment => Ascent(segment.Font));
                    foreach (var segment in line)
                    {
                        gfx.DrawString(segment.Text, segment.Font, segment.Brush, lineX, baseline, BaselineFormat);
                        if (segment.Run is { } run)
                            Recorder?.Record(run.Anchor, run.Link, new XRect(lineX, lineTop, segment.Width, lineHeight));
                        lineX += segment.Width;
                    }

                    lastLineEnd = lineX;
                }
                else
                {
                    var segment = line[0];
                    gfx.DrawString(segment.Text, segment.Font, segment.Brush,
                        new XRect(left, lineTop, cell.TextWidth, lineHeight), format);
                    lastLineEnd = left + gfx.MeasureString(segment.Text, segment.Font).Width;
                }

                if (li == cell.Lines.Count - 1 && cellStyle?.Leader == true)
                    DrawLeader(gfx, cellStyle.LeaderColor, lastLineEnd, currentX + columnWidths[i] - cellPadding,
                        lineTop + Ascent(line[0].Font));

                lineTop += lineHeight;
            }

            if (cellStyle?.BorderBottomColor != null)
            {
                var width = UnitConverter.ToPoints(cellStyle.BorderBottomWidth, 0.75);
                gfx.DrawLine(new XPen(ColorParser.Parse(cellStyle.BorderBottomColor), width),
                    currentX, y + rowHeight - width / 2, currentX + columnWidths[i], y + rowHeight - width / 2);
            }

            if (borderPen != null)
                gfx.DrawRectangle(borderPen, currentX, y, columnWidths[i], rowHeight);

            currentX += columnWidths[i];
        }

        return rowHeight;
    }

    private static readonly XStringFormat BaselineFormat = new()
    {
        Alignment = XStringAlignment.Near,
        LineAlignment = XLineAlignment.BaseLine
    };

    private const double DotSize = 5;
    private const double DotGap = 4;
    private const double LeaderGap = 6;

    /// <summary>
    ///     A rounded label as wide as the longest line plus its padding (and dot), placed like the text
    ///     (left, centred or right) within the content area of the cell.
    /// </summary>
    private static void DrawBadge(XGraphics gfx, XBrush brush, string? dotColor, CellLayout cell,
        XStringFormat format, double left, double top, double width)
    {
        var lineWidths = cell.Lines.Select(line => line.Sum(segment => segment.Text.Length == 0
            ? 0
            : gfx.MeasureString(segment.Text, segment.Font).Width)).ToList();
        if (lineWidths.All(lineWidth => lineWidth == 0))
            return;

        var dot = dotColor != null ? DotSize + DotGap : 0;
        var badgeWidth = Math.Min(width, lineWidths.Max() + dot + BadgePaddingX * 2);
        var badgeHeight = cell.Height + BadgePaddingY * 2;
        var badgeX = format.Alignment switch
        {
            XStringAlignment.Center => left + (width - badgeWidth) / 2,
            XStringAlignment.Far => left + width - badgeWidth,
            _ => left
        };

        var corner = Math.Min(badgeHeight, cell.LineHeights[0] + BadgePaddingY * 2);
        gfx.DrawRoundedRectangle(brush, badgeX, top - BadgePaddingY, badgeWidth, badgeHeight, corner, corner);

        if (dotColor != null)
            gfx.DrawEllipse(new XSolidBrush(ColorParser.Parse(dotColor)), badgeX + BadgePaddingX,
                top + (cell.LineHeights[0] - DotSize) / 2, DotSize, DotSize);
    }

    /// <summary>A dotted line from the end of the text to the right edge of the cell, on the baseline.</summary>
    private static void DrawLeader(XGraphics gfx, string? color, double from, double to, double baseline)
    {
        from += LeaderGap;
        to -= LeaderGap;
        if (to - from < LeaderGap)
            return;

        var pen = new XPen(ColorParser.Parse(color, XColor.FromArgb(0xB9, 0xC0, 0xCD)), 1) { DashStyle = XDashStyle.Dot };
        gfx.DrawLine(pen, from, baseline, to, baseline);
    }

    private static double Ascent(XFont font) => font.GetHeight() * font.CellAscent / font.CellSpace;

    /// <summary>A piece of a line of a cell, with its font, color and width.</summary>
    private sealed record Segment(string Text, XFont Font, XBrush Brush, double Width, TextRun? Run = null);

    /// <summary>The lines of a cell and their heights; a line of a plain cell is a single segment.</summary>
    private sealed record CellLayout(List<List<Segment>> Lines, List<double> LineHeights, double TextWidth, bool IsRuns)
    {
        public double Height => LineHeights.Sum();
    }

    private static CellLayout LayoutCell(XGraphics gfx, TableElement table, TableRow row, int index, double columnWidth,
        double cellPadding, ResolvedStyle? style)
    {
        var cellStyle = CellStyleAt(row, index);
        var font = CellFont(table, row, cellStyle, style);
        var brush = new XSolidBrush(ColorParser.Parse(cellStyle?.TextColor ?? row.TextColor, XColors.Black));
        var textWidth = TextWidth(columnWidth, cellPadding, cellStyle);

        if (cellStyle?.Runs is { Count: > 0 } runs)
        {
            var lines = WrapRuns(gfx, runs, table, row, cellStyle, style, brush, textWidth);
            return new CellLayout(lines, lines.Select(line => line.Count == 0 ? (double)font.Height : line.Max(segment => (double)segment.Font.Height)).ToList(), textWidth, true);
        }

        var plain = WrapCell(gfx, row.Cells[index], font, textWidth)
            .Select(line => new List<Segment> { new(line, font, brush, 0) })
            .ToList();
        return new CellLayout(plain, plain.Select(_ => (double)font.Height).ToList(), textWidth, false);
    }

    /// <summary>
    ///     The runs of a cell over lines: word by word, each word with the style of its run, a new line
    ///     where a run has a newline or the next word does not fit.
    /// </summary>
    private static List<List<Segment>> WrapRuns(XGraphics gfx, IEnumerable<TextRun> runs, TableElement table,
        TableRow row, TableCellStyle cellStyle, ResolvedStyle? style, XBrush cellBrush, double maxWidth)
    {
        var lines = new List<List<Segment>> { new() };
        var lineWidth = 0d;

        foreach (var run in runs)
        {
            var font = new XFont(style?.FontFamily ?? FontFamilies.Helvetica,
                run.FontSize ?? cellStyle.FontSize ?? table.DataFontSize ?? PdfDefaults.TableDataFontSize,
                run.IsBold ?? cellStyle.IsBold ?? row.IsBold ? XFontStyle.Bold : XFontStyle.Regular);
            XBrush brush = run.Color != null ? new XSolidBrush(ColorParser.Parse(run.Color)) : cellBrush;

            var parts = (run.Text ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            for (var p = 0; p < parts.Length; p++)
            {
                if (p > 0)
                {
                    lines.Add(new List<Segment>());
                    lineWidth = 0;
                }

                // Words with the spaces before them, so the spacing of a run is kept.
                foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(parts[p], @"\s*\S+|\s+$"))
                {
                    var text = lines[^1].Count == 0 ? match.Value.TrimStart() : match.Value;
                    if (text.Length == 0)
                        continue;

                    var width = gfx.MeasureString(text, font).Width;
                    if (lineWidth + width > maxWidth && lines[^1].Count > 0)
                    {
                        lines.Add(new List<Segment>());
                        text = text.TrimStart();
                        width = gfx.MeasureString(text, font).Width;
                        lineWidth = 0;
                    }

                    lines[^1].Add(new Segment(text, font, brush, width, run));
                    lineWidth += width;
                }
            }
        }

        return lines;
    }

    private static TableCellStyle? CellStyleAt(TableRow row, int index)
    {
        return row.CellStyles != null && index < row.CellStyles.Count ? row.CellStyles[index] : null;
    }

    /// <summary>The font of a cell: bold when its style says so, otherwise when its row is bold.</summary>
    private static XFont CellFont(TableElement table, TableRow row, TableCellStyle? cellStyle, ResolvedStyle? style)
    {
        var bold = cellStyle?.IsBold ?? row.IsBold;
        return new XFont(style?.FontFamily ?? FontFamilies.Helvetica,
            cellStyle?.FontSize ?? table.DataFontSize ?? PdfDefaults.TableDataFontSize,
            bold ? XFontStyle.Bold : XFontStyle.Regular);
    }

    /// <summary>The width the text of a cell wraps in; a badge keeps room for its padding.</summary>
    private static double TextWidth(double columnWidth, double cellPadding, TableCellStyle? cellStyle)
    {
        var inset = cellStyle?.Badge == true ? BadgeInset(cellStyle) + BadgePaddingX : 0;
        return Math.Max(1, columnWidth - cellPadding * 2 - inset);
    }

    /// <summary>Where the text of a badge starts: after its padding and its dot.</summary>
    private static double BadgeInset(TableCellStyle? cellStyle) =>
        cellStyle?.Badge != true ? 0 : BadgePaddingX + (cellStyle.DotColor != null ? DotSize + DotGap : 0);

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
