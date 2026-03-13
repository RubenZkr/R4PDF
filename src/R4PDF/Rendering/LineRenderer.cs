using R4PDF.Models.Elements;
using R4PDF.Parsing;
using PdfSharpCore.Drawing;

namespace R4PDF.Rendering;

public class LineRenderer
{
    public double Render(XGraphics gfx, LineElement element, double x, double y, double availableWidth)
    {
        var color = ColorParser.Parse(element.Color, XColors.Black);
        var strokeWidth = UnitConverter.ToPoints(element.StrokeWidth, 1.0);
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
            x1 = UnitConverter.ToPoints(element.X1);
            y1 = UnitConverter.ToPoints(element.Y1);
            x2 = UnitConverter.ToPoints(element.X2);
            y2 = UnitConverter.ToPoints(element.Y2);
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
