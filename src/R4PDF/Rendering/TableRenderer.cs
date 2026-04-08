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

    public double Render(XGraphics gfx, TableElement table, ResolvedStyle style, double x, double y,
        double availableWidth)
    {
        var result = RenderWithContinuation(gfx, table, style, x, y, availableWidth, double.MaxValue, true);
        return result.HeightUsed;
    }

    public SplitRenderResult RenderWithContinuation(XGraphics gfx, TableElement table, ResolvedStyle style,
        double x, double y, double availableWidth, double availableHeight, bool allowSplit)
    {
        if (table.Columns.Count == 0)
            return new SplitRenderResult { HeightUsed = 0, RenderedPart = CloneTable(table), Remainder = null };

        var columnWidths = CalculateColumnWidths(table, availableWidth);
        var borderPen = ResolveBorderPen(table.Borders);
        var currentY = y;
        var heightUsed = 0d;
        var renderedRows = new List<TableRow>();
        var remainderRows = new List<TableRow>();

        var headerHeight = table.ShowHeader ? MeasureHeaderHeight(gfx, table, columnWidths) : 0;
        if (table.ShowHeader && headerHeight > availableHeight)
            return new SplitRenderResult { HeightUsed = 0, RenderedPart = null, Remainder = CloneTable(table) };

        if (!allowSplit)
        {
            var fullHeight = headerHeight + MeasureRowsHeight(gfx, table, columnWidths, 0, table.Rows.Count);
            if (fullHeight > availableHeight)
                return new SplitRenderResult { HeightUsed = 0, RenderedPart = null, Remainder = CloneTable(table) };
        }

        if (table.ShowHeader)
        {
            currentY += RenderHeaderRow(gfx, table, columnWidths, x, currentY, borderPen);
            heightUsed += headerHeight;
        }

        for (var i = 0; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];
            var rowHeight = MeasureDataRowHeight(gfx, table, row, columnWidths);
            var remainingHeight = availableHeight - heightUsed;

            if (rowHeight > remainingHeight)
            {
                remainderRows.Add(CloneRow(row));
                for (var j = i + 1; j < table.Rows.Count; j++)
                    remainderRows.Add(CloneRow(table.Rows[j]));
                break;
            }

            XBrush? rowBackground = null;
            if (row.BackgroundColor != null)
                rowBackground = new XSolidBrush(ColorParser.Parse(row.BackgroundColor));
            else if (table.AlternateRowColors && i % 2 == 1)
                rowBackground =
                    new XSolidBrush(ColorParser.Parse(table.AlternateColor, XColor.FromArgb(245, 245, 245)));

            currentY += RenderDataRow(gfx, table, row, columnWidths, x, currentY, borderPen, rowBackground);
            heightUsed += rowHeight;
            renderedRows.Add(CloneRow(row));
        }

        var renderedPart = renderedRows.Count > 0 || table.ShowHeader
            ? CloneTable(table, renderedRows)
            : null;

        var remainder = remainderRows.Count > 0
            ? CloneTable(table, remainderRows)
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
            var wrapped = WrapText(gfx, table.Columns[i].Name, headerFont, cellWidth);
            if (wrapped.Count > maxLines)
                maxLines = wrapped.Count;
        }

        return lineHeight * maxLines + DefaultCellPadding * 2;
    }

    private double MeasureRowsHeight(XGraphics gfx, TableElement table, double[] columnWidths, int start, int count)
    {
        var total = 0d;
        for (var i = start; i < start + count; i++)
            total += MeasureDataRowHeight(gfx, table, table.Rows[i], columnWidths);
        return total;
    }

    private double MeasureDataRowHeight(XGraphics gfx, TableElement table, TableRow row, double[] columnWidths)
    {
        var font = new XFont(FontFamilies.Helvetica, PdfDefaults.TableDataFontSize);
        var lineHeight = font.Height;
        var cellCount = Math.Min(row.Cells.Count, table.Columns.Count);
        var maxLines = 1;

        for (var i = 0; i < cellCount; i++)
        {
            var cellText = row.Cells[i] ?? "";
            var cellWidth = columnWidths[i] - DefaultCellPadding * 2;
            var wrapped = WrapText(gfx, cellText, font, cellWidth);
            if (wrapped.Count > maxLines)
                maxLines = wrapped.Count;
        }

        return lineHeight * maxLines + DefaultCellPadding * 2;
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
            wrappedHeaders[i] = WrapText(gfx, table.Columns[i].Name, headerFont, cellWidth);
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
        double x, double y, XPen? borderPen, XBrush? rowBackground)
    {
        var font = new XFont(FontFamilies.Helvetica, PdfDefaults.TableDataFontSize);
        var textBrush = new XSolidBrush(ColorParser.Parse(row.TextColor, XColors.Black));
        var lineHeight = font.Height;

        var cellCount = Math.Min(row.Cells.Count, table.Columns.Count);
        var wrappedCells = new List<string>[cellCount];
        var maxLines = 1;

        for (var i = 0; i < cellCount; i++)
        {
            var cellText = row.Cells[i] ?? "";
            var cellWidth = columnWidths[i] - DefaultCellPadding * 2;
            wrappedCells[i] = WrapText(gfx, cellText, font, cellWidth);
            if (wrappedCells[i].Count > maxLines)
                maxLines = wrappedCells[i].Count;
        }

        var rowHeight = lineHeight * maxLines + DefaultCellPadding * 2;
        var currentX = x;

        if (rowBackground != null)
            gfx.DrawRectangle(rowBackground, x, y, columnWidths.Sum(), rowHeight);

        for (var i = 0; i < cellCount; i++)
        {
            var alignment = i < table.Columns.Count ? table.Columns[i].Alignment : null;
            var format = GetCellFormat(alignment);
            var lines = wrappedCells[i];

            for (var li = 0; li < lines.Count; li++)
            {
                var lineRect = new XRect(currentX + DefaultCellPadding, y + DefaultCellPadding + li * lineHeight,
                    columnWidths[i] - DefaultCellPadding * 2, lineHeight);
                gfx.DrawString(lines[li], font, textBrush, lineRect, format);
            }

            if (borderPen != null)
                gfx.DrawRectangle(borderPen, currentX, y, columnWidths[i], rowHeight);

            currentX += columnWidths[i];
        }

        return rowHeight;
    }

    private static XPen? ResolveBorderPen(BorderStyle? borders)
    {
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

    private static List<string> WrapText(XGraphics gfx, string text, XFont font, double maxWidth)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text))
        {
            lines.Add(string.Empty);
            return lines;
        }

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = currentLine.Length == 0 ? word : $"{currentLine} {word}";
            var size = gfx.MeasureString(testLine, font);

            if (size.Width > maxWidth && currentLine.Length > 0)
            {
                lines.Add(currentLine);
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (currentLine.Length > 0)
            lines.Add(currentLine);

        if (lines.Count == 0)
            lines.Add(string.Empty);

        return lines;
    }

    private static TableElement CloneTable(TableElement source, List<TableRow>? rows = null)
    {
        return new TableElement
        {
            Style = source.Style,
            InlineStyle = source.InlineStyle,
            X = source.X,
            Y = source.Y,
            Width = source.Width,
            Height = source.Height,
            ShowHeader = source.ShowHeader,
            AlternateRowColors = source.AlternateRowColors,
            AlternateColor = source.AlternateColor,
            HeaderStyle = source.HeaderStyle == null
                ? null
                : new PdfStyle
                {
                    FontFamily = source.HeaderStyle.FontFamily,
                    FontSize = source.HeaderStyle.FontSize,
                    FontWeight = source.HeaderStyle.FontWeight,
                    FontStyle = source.HeaderStyle.FontStyle,
                    Color = source.HeaderStyle.Color,
                    BackgroundColor = source.HeaderStyle.BackgroundColor,
                    Alignment = source.HeaderStyle.Alignment,
                    LineHeight = source.HeaderStyle.LineHeight
                },
            Borders = source.Borders == null
                ? null
                : new BorderStyle
                {
                    Type = source.Borders.Type,
                    Color = source.Borders.Color,
                    Width = source.Borders.Width
                },
            Columns = source.Columns.Select(c => new TableColumn
            {
                Name = c.Name,
                Width = c.Width,
                Alignment = c.Alignment
            }).ToList(),
            Rows = rows ?? source.Rows.Select(CloneRow).ToList()
        };
    }

    private static TableRow CloneRow(TableRow source)
    {
        return new TableRow
        {
            Cells = source.Cells.ToList(),
            BackgroundColor = source.BackgroundColor,
            TextColor = source.TextColor
        };
    }
}
