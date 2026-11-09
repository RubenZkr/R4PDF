using PdfSharpCore.Drawing;
using R4PDF.Models.Elements;
using R4PDF.Parsing;

namespace R4PDF.Rendering;

public class LineRenderer
{
    /// <param name="originX">Added to X1 and X2, for a line in a canvas.</param>
    /// <param name="originY">Added to Y1 and Y2, for a line in a canvas.</param>
    public double Render(XGraphics gfx, LineElement element, double x, double y, double availableWidth,
        double originX = 0, double originY = 0)
    {
        var color = ColorParser.Parse(element.Color, XColors.Black);
        var strokeWidth = UnitConverter.ToPoints(element.StrokeWidth, PdfDefaults.DefaultStrokeWidth);
        var pen = new XPen(color, strokeWidth);

        // Parse dash pattern if specified (e.g., "2,2" or "4,2,1,2")
        if (!string.IsNullOrEmpty(element.DashPattern))
        {
            var parts = element.DashPattern.Split(',');
            if (parts.Length >= 2)
            {
                pen.DashStyle = XDashStyle.Custom;
                pen.DashPattern = parts.Select(p => double.Parse(p.Trim())).ToArray();
            }
        }

        double x1, y1, x2, y2;

        if (element.X1 != null && element.Y1 != null && element.X2 != null && element.Y2 != null)
        {
            // Absolute coordinates
            x1 = originX + UnitConverter.ToPoints(element.X1);
            y1 = originY + UnitConverter.ToPoints(element.Y1);
            x2 = originX + UnitConverter.ToPoints(element.X2);
            y2 = originY + UnitConverter.ToPoints(element.Y2);
        }
        else
        {
            // Default: horizontal line across available width at current Y position
            x1 = x;
            y1 = y;
            x2 = x + availableWidth;
            y2 = y;
        }

        gfx.DrawLine(pen, x1, y1, x2, y2);

        // Return the vertical space consumed
        return Math.Abs(y2 - y1) + strokeWidth;
    }
}