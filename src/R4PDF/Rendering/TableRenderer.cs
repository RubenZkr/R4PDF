using R4PDF.Models;
using R4PDF.Models.Elements;
using R4PDF.Parsing;
using PdfSharpCore.Drawing;

namespace R4PDF.Rendering;

public class TableRenderer
{
    private const double DefaultCellPadding = 4;

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
        var headerFont = new XFont("Helvetica", 12, XFontStyle.Bold);
        var headerBrush = XBrushes.White;
        var headerBgColor = XColors.DarkGray;

        if (table.HeaderStyle != null)
        {
            var fontFamily = table.HeaderStyle.FontFamily ?? "Helvetica";
            var fontSize = table.HeaderStyle.FontSize ?? 12;
            var fontStyle = XFontStyle.Regular;
            if (table.HeaderStyle.FontWeight?.Equals("bold", StringComparison.OrdinalIgnoreCase) == true)
                fontStyle |= XFontStyle.Bold;

            headerFont = new XFont(fontFamily, fontSize, fontStyle);
            headerBrush = new XSolidBrush(ColorParser.Parse(table.HeaderStyle.Color, XColors.White));
            headerBgColor = ColorParser.Parse(table.HeaderStyle.BackgroundColor, XColors.DarkGray);
        }

        var rowHeight = headerFont.Height + DefaultCellPadding * 2;
        double currentX = x;

        // Draw header background
        gfx.DrawRectangle(new XSolidBrush(headerBgColor), x, y, columnWidths.Sum(), rowHeight);

        // Draw header cells
        for (int i = 0; i < table.Columns.Count; i++)
        {
            var cellRect = new XRect(currentX + DefaultCellPadding, y + DefaultCellPadding,
                columnWidths[i] - DefaultCellPadding * 2, rowHeight - DefaultCellPadding * 2);

            var format = GetCellFormat(table.Columns[i].Alignment);
            gfx.DrawString(table.Columns[i].Name, headerFont, headerBrush, cellRect, format);

            if (borderPen != null)
                gfx.DrawRectangle(borderPen, currentX, y, columnWidths[i], rowHeight);

            currentX += columnWidths[i];
        }

        return rowHeight;
    }

    private double RenderDataRow(XGraphics gfx, TableElement table, TableRow row, double[] columnWidths,
        double x, double y, XPen? borderPen, XBrush? rowBackground)
    {
        var font = new XFont("Helvetica", 11);
        var textBrush = new XSolidBrush(ColorParser.Parse(row.TextColor, XColors.Black));
        var rowHeight = font.Height + DefaultCellPadding * 2;
        double currentX = x;

        // Draw row background
        if (rowBackground != null)
        {
            gfx.DrawRectangle(rowBackground, x, y, columnWidths.Sum(), rowHeight);
        }

        // Draw cells
        for (int i = 0; i < Math.Min(row.Cells.Count, table.Columns.Count); i++)
        {
            var cellRect = new XRect(currentX + DefaultCellPadding, y + DefaultCellPadding,
                columnWidths[i] - DefaultCellPadding * 2, rowHeight - DefaultCellPadding * 2);

            var alignment = i < table.Columns.Count ? table.Columns[i].Alignment : null;
            var format = GetCellFormat(alignment);
            gfx.DrawString(row.Cells[i] ?? "", font, textBrush, cellRect, format);

            if (borderPen != null)
                gfx.DrawRectangle(borderPen, currentX, y, columnWidths[i], rowHeight);

            currentX += columnWidths[i];
        }

        return rowHeight;
    }

    private static XPen? ResolveBorderPen(BorderStyle? borders)
    {
        if (borders == null)
            return new XPen(XColors.LightGray, 0.5);

        var color = ColorParser.Parse(borders.Color, XColors.LightGray);
        var width = UnitConverter.ToPoints(borders.Width, 0.5);
        return new XPen(color, width);
    }

    private static XStringFormat GetCellFormat(string? alignment)
    {
        var format = new XStringFormat { LineAlignment = XLineAlignment.Center };
        format.Alignment = alignment?.ToLowerInvariant() switch
        {
            "center" => XStringAlignment.Center,
            "right" => XStringAlignment.Far,
            _ => XStringAlignment.Near,
        };
        return format;
    }
}
