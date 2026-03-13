using R4PDF.Models.Elements;
using R4PDF.Parsing;
using PdfSharpCore.Drawing;

namespace R4PDF.Rendering;

public class TextRenderer
{
    public double Render(XGraphics gfx, TextElement element, ResolvedStyle style, double x, double y, double availableWidth)
    {
        var font = style.ToXFont();
        var brush = new XSolidBrush(ColorParser.Parse(style.Color, XColors.Black));
        var format = style.ToXStringFormat();

        // Calculate layout rect
        var width = element.Width != null ? UnitConverter.ToPoints(element.Width) : availableWidth;
        var textSize = gfx.MeasureString(element.Text, font);
        var height = element.Height != null ? UnitConverter.ToPoints(element.Height) : textSize.Height;

        var rect = new XRect(x, y, width, height);
        gfx.DrawString(element.Text, font, brush, rect, format);

        return height;
    }
}
