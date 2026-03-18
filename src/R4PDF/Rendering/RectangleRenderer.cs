using PdfSharpCore.Drawing;
using R4PDF.Models.Elements;
using R4PDF.Parsing;

namespace R4PDF.Rendering;

public class RectangleRenderer
{
    public double Render(XGraphics gfx, RectangleElement element, double x, double y, double availableWidth)
    {
        var width = element.Width != null ? UnitConverter.ToPoints(element.Width) : availableWidth;
        var height = element.Height != null
            ? UnitConverter.ToPoints(element.Height)
            : PdfDefaults.DefaultRectangleHeight;

        var drawX = element.X != null ? UnitConverter.ToPoints(element.X) : x;
        var drawY = element.Y != null ? UnitConverter.ToPoints(element.Y) : y;

        XBrush? fillBrush = element.FillColor != null
            ? new XSolidBrush(ColorParser.Parse(element.FillColor))
            : null;

        XPen? strokePen = null;
        if (element.StrokeColor != null)
        {
            var strokeWidth = UnitConverter.ToPoints(element.StrokeWidth, PdfDefaults.DefaultStrokeWidth);
            strokePen = new XPen(ColorParser.Parse(element.StrokeColor), strokeWidth);
        }

        var cornerRadius = UnitConverter.ToPoints(element.CornerRadius, 0);

        if (cornerRadius > 0)
        {
            // Rounded rectangle — approximate with DrawRoundedRectangle
            if (fillBrush != null && strokePen != null)
                gfx.DrawRoundedRectangle(strokePen, fillBrush, drawX, drawY, width, height, cornerRadius, cornerRadius);
            else if (fillBrush != null)
                gfx.DrawRoundedRectangle(fillBrush, drawX, drawY, width, height, cornerRadius, cornerRadius);
            else if (strokePen != null)
                gfx.DrawRoundedRectangle(strokePen, drawX, drawY, width, height, cornerRadius, cornerRadius);
        }
        else
        {
            if (fillBrush != null && strokePen != null)
                gfx.DrawRectangle(strokePen, fillBrush, drawX, drawY, width, height);
            else if (fillBrush != null)
                gfx.DrawRectangle(fillBrush, drawX, drawY, width, height);
            else if (strokePen != null)
                gfx.DrawRectangle(strokePen, drawX, drawY, width, height);
        }

        return height;
    }
}