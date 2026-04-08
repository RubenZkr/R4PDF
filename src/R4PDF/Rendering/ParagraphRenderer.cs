using PdfSharpCore.Drawing;
using R4PDF.Models.Elements;
using R4PDF.Parsing;

namespace R4PDF.Rendering;

public class ParagraphRenderer
{
    public sealed class SplitRenderResult
    {
        public double HeightUsed { get; init; }
        public ParagraphElement? RenderedPart { get; init; }
        public ParagraphElement? Remainder { get; init; }
    }

    public double Render(XGraphics gfx, ParagraphElement element, ResolvedStyle style, double x, double y,
        double availableWidth)
    {
        var result = RenderWithContinuation(gfx, element, style, x, y, availableWidth, double.MaxValue, true);
        return result.HeightUsed;
    }

    public SplitRenderResult RenderWithContinuation(XGraphics gfx, ParagraphElement element, ResolvedStyle style,
        double x, double y, double availableWidth, double availableHeight, bool allowSplit)
    {
        var font = style.ToXFont();
        var brush = new XSolidBrush(ColorParser.Parse(style.Color, XColors.Black));
        var width = element.Width != null ? UnitConverter.ToPoints(element.Width) : availableWidth;
        var lineHeight = style.LineHeight * style.FontSize;

        double spaceBefore = 0;
        if (element.Spacing?.Before != null)
            spaceBefore = UnitConverter.ToPoints(element.Spacing.Before);

        double spaceAfter = 0;
        if (element.Spacing?.After != null)
            spaceAfter = UnitConverter.ToPoints(element.Spacing.After);

        var lines = WrapText(gfx, element.Content, font, width);
        var fullHeight = spaceBefore + lines.Count * lineHeight + spaceAfter;

        if (!allowSplit)
        {
            if (fullHeight > availableHeight)
                return new SplitRenderResult { HeightUsed = 0, RenderedPart = null, Remainder = CloneParagraph(element) };

            RenderLines(gfx, lines, font, brush, style, x, y, width, lineHeight, spaceBefore);
            return new SplitRenderResult
            {
                HeightUsed = fullHeight,
                RenderedPart = CloneParagraph(element),
                Remainder = null
            };
        }

        if (fullHeight <= availableHeight)
        {
            RenderLines(gfx, lines, font, brush, style, x, y, width, lineHeight, spaceBefore);
            return new SplitRenderResult
            {
                HeightUsed = fullHeight,
                RenderedPart = CloneParagraph(element),
                Remainder = null
            };
        }

        var usableHeight = availableHeight - spaceBefore;
        var maxLines = usableHeight > 0 ? (int)Math.Floor(usableHeight / lineHeight) : 0;

        if (maxLines <= 0)
            return new SplitRenderResult { HeightUsed = 0, RenderedPart = null, Remainder = CloneParagraph(element) };

        if (maxLines > lines.Count)
            maxLines = lines.Count;

        var renderedLines = lines.Take(maxLines).ToList();
        var remainingLines = lines.Skip(maxLines).ToList();

        RenderLines(gfx, renderedLines, font, brush, style, x, y, width, lineHeight, spaceBefore);

        return new SplitRenderResult
        {
            HeightUsed = spaceBefore + renderedLines.Count * lineHeight,
            RenderedPart = CloneParagraph(element, string.Join("\n", renderedLines)),
            Remainder = remainingLines.Count > 0 ? CloneParagraph(element, string.Join("\n", remainingLines)) : null
        };
    }

    private static void RenderLines(XGraphics gfx, List<string> lines, XFont font, XBrush brush, ResolvedStyle style,
        double x, double y, double width, double lineHeight, double spaceBefore)
    {
        var currentY = y + spaceBefore;
        var format = style.ToXStringFormat();

        foreach (var line in lines)
        {
            var rect = new XRect(x, currentY, width, lineHeight);
            gfx.DrawString(line, font, brush, rect, format);
            currentY += lineHeight;
        }
    }

    private static ParagraphElement CloneParagraph(ParagraphElement source, string? content = null)
    {
        return new ParagraphElement
        {
            Style = source.Style,
            InlineStyle = source.InlineStyle,
            X = source.X,
            Y = source.Y,
            Width = source.Width,
            Height = source.Height,
            Content = content ?? source.Content,
            FontFamily = source.FontFamily,
            FontSize = source.FontSize,
            FontWeight = source.FontWeight,
            Color = source.Color,
            Alignment = source.Alignment,
            LineHeight = source.LineHeight,
            Spacing = source.Spacing == null
                ? null
                : new SpacingSettings
                {
                    Before = source.Spacing.Before,
                    After = source.Spacing.After,
                    LineHeight = source.Spacing.LineHeight
                }
        };
    }

    private static List<string> WrapText(XGraphics gfx, string text, XFont font, double maxWidth)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text))
            return lines;

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
