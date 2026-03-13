using R4PDF.Models.Elements;
using R4PDF.Parsing;
using PdfSharpCore.Drawing;

namespace R4PDF.Rendering;

public class ParagraphRenderer
{
    public double Render(XGraphics gfx, ParagraphElement element, ResolvedStyle style, double x, double y, double availableWidth)
    {
        var font = style.ToXFont();
        var brush = new XSolidBrush(ColorParser.Parse(style.Color, XColors.Black));
        var width = element.Width != null ? UnitConverter.ToPoints(element.Width) : availableWidth;
        var lineHeight = style.LineHeight * style.FontSize;

        // Handle spacing before
        double spaceBefore = 0;
        if (element.Spacing?.Before != null)
            spaceBefore = UnitConverter.ToPoints(element.Spacing.Before);

        double currentY = y + spaceBefore;

        // Word-wrap the text
        var lines = WrapText(gfx, element.Content, font, width);
        var format = style.ToXStringFormat();

        foreach (var line in lines)
        {
            var rect = new XRect(x, currentY, width, lineHeight);
            gfx.DrawString(line, font, brush, rect, format);
            currentY += lineHeight;
        }

        // Handle spacing after
        double spaceAfter = 0;
        if (element.Spacing?.After != null)
            spaceAfter = UnitConverter.ToPoints(element.Spacing.After);

        return (currentY - y) + spaceAfter;
    }

    private static List<string> WrapText(XGraphics gfx, string text, XFont font, double maxWidth)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text))
            return lines;

        // Split by explicit newlines first
        var paragraphs = text.Split('\n');

        foreach (var paragraph in paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph))
            {
                lines.Add(string.Empty);
                continue;
            }

            var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
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
        }

        return lines;
    }
}
