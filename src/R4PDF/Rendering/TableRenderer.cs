using R4PDF.Models;
using R4PDF.Models.Elements;
using R4PDF.Parsing;
using PdfSharpCore.Drawing;

namespace R4PDF.Rendering;

public class TableRenderer
{
    private const double DefaultCellPadding = PdfDefaults.TableCellPadding;

    public double Render(XGraphics gfx, TableElement table, ResolvedStyle style, double x, double y, double availableWidth)
    {
        if (table.Columns.Count == 0)
            return 0;

        // Calculate column widths
        var columnWidths = CalculateColumnWidths(table, availableWidth);
        double currentY = y;

        // Resolve border style
        var borderPen = ResolveBorderPen(table.Borders);

        // Render header row
        if (table.ShowHeader)
        {
            currentY += RenderHeaderRow(gfx, table, columnWidths, x, currentY, borderPen);
        }

        // Render data rows
        for (int i = 0; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];

            // Alternate row coloring
            XBrush? rowBackground = null;
            if (row.BackgroundColor != null)
            {
                rowBackground = new XSolidBrush(ColorParser.Parse(row.BackgroundColor));
            }
            else if (table.AlternateRowColors && i % 2 == 1)
            {
                rowBackground = new XSolidBrush(ColorParser.Parse(table.AlternateColor, XColor.FromArgb(245, 245, 245)));
            }

            currentY += RenderDataRow(gfx, table, row, columnWidths, x, currentY, borderPen, rowBackground);
        }

        return currentY - y;
    }

    private double[] CalculateColumnWidths(TableElement table, double availableWidth)
    {
        var widths = new double[table.Columns.Count];
        double totalFixed = 0;
        int autoCount = 0;

        for (int i = 0; i < table.Columns.Count; i++)
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

        // Distribute remaining width to auto columns
        if (autoCount > 0)
        {
            var remaining = Math.Max(0, availableWidth - totalFixed) / autoCount;
            for (int i = 0; i < widths.Length; i++)
            {
                if (widths[i] == 0)
                    widths[i] = remaining;
            }
        }

        return widths;
    }

    private double RenderHeaderRow(XGraphics gfx, TableElement table, double[] columnWidths, double x, double y, XPen? borderPen)
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

        // Pre-compute wrapped lines for each header cell to determine row height
        var wrappedHeaders = new List<string>[table.Columns.Count];
        int maxLines = 1;
        for (int i = 0; i < table.Columns.Count; i++)
        {
            var cellWidth = columnWidths[i] - DefaultCellPadding * 2;
            wrappedHeaders[i] = WrapText(gfx, table.Columns[i].Name, headerFont, cellWidth);
            if (wrappedHeaders[i].Count > maxLines)
                maxLines = wrappedHeaders[i].Count;
        }

        var rowHeight = lineHeight * maxLines + DefaultCellPadding * 2;
        double currentX = x;

        // Draw header background
        gfx.DrawRectangle(new XSolidBrush(headerBgColor), x, y, columnWidths.Sum(), rowHeight);

        // Draw header cells with wrapped text
        for (int i = 0; i < table.Columns.Count; i++)
        {
            var format = GetCellFormat(table.Columns[i].Alignment);
            var lines = wrappedHeaders[i];

            for (int li = 0; li < lines.Count; li++)
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

        // Pre-compute wrapped lines for each cell to determine row height
        var cellCount = Math.Min(row.Cells.Count, table.Columns.Count);
        var wrappedCells = new List<string>[cellCount];
        int maxLines = 1;

        for (int i = 0; i < cellCount; i++)
        {
            var cellText = row.Cells[i] ?? "";
            var cellWidth = columnWidths[i] - DefaultCellPadding * 2;
            wrappedCells[i] = WrapText(gfx, cellText, font, cellWidth);
            if (wrappedCells[i].Count > maxLines)
                maxLines = wrappedCells[i].Count;
        }

        var rowHeight = lineHeight * maxLines + DefaultCellPadding * 2;
        double currentX = x;

        // Draw row background
        if (rowBackground != null)
        {
            gfx.DrawRectangle(rowBackground, x, y, columnWidths.Sum(), rowHeight);
        }

        // Draw cells with wrapped text
        for (int i = 0; i < cellCount; i++)
        {
            var alignment = i < table.Columns.Count ? table.Columns[i].Alignment : null;
            var format = GetCellFormat(alignment);
            var lines = wrappedCells[i];

            for (int li = 0; li < lines.Count; li++)
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
            _ => XStringAlignment.Near,
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
}
