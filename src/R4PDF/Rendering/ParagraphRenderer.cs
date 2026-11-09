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

    /// <summary>
    ///     The height of the whole paragraph and of its start (spacing before and the first line), without
    ///     drawing it; used to keep a heading together with the paragraph that follows it.
    /// </summary>
    public (double Full, double Start) Measure(XGraphics gfx, ParagraphElement element, ResolvedStyle style,
        double availableWidth)
    {
        var font = style.ToXFont();
        var width = element.Width != null ? UnitConverter.ToPoints(element.Width) : availableWidth;
        var lineHeight = style.LineHeight * style.FontSize;
        var spaceBefore = element.Spacing?.Before != null ? UnitConverter.ToPoints(element.Spacing.Before) : 0;
        var spaceAfter = element.Spacing?.After != null ? UnitConverter.ToPoints(element.Spacing.After) : 0;

        var lines = TextWrapper.Wrap(gfx, element.Content, font, width);
        var full = spaceBefore + lines.Count * lineHeight + spaceAfter;
        return (full, lines.Count > 1 ? spaceBefore + lineHeight : full);
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

        var lines = TextWrapper.Wrap(gfx, element.Content, font, width);
        var fullHeight = spaceBefore + lines.Count * lineHeight + spaceAfter;

        if (!allowSplit)
        {
            if (fullHeight > availableHeight)
                return new SplitRenderResult { HeightUsed = 0, RenderedPart = null, Remainder = (ParagraphElement)element.Clone() };

            RenderLines(gfx, lines, font, brush, style, x, y, width, lineHeight, spaceBefore);
            return new SplitRenderResult
            {
                HeightUsed = fullHeight,
                RenderedPart = (ParagraphElement)element.Clone(),
                Remainder = null
            };
        }

        if (fullHeight <= availableHeight)
        {
            RenderLines(gfx, lines, font, brush, style, x, y, width, lineHeight, spaceBefore);
            return new SplitRenderResult
            {
                HeightUsed = fullHeight,
                RenderedPart = (ParagraphElement)element.Clone(),
                Remainder = null
            };
        }

        var usableHeight = availableHeight - spaceBefore;
        var maxLines = usableHeight > 0 ? (int)Math.Floor(usableHeight / lineHeight) : 0;

        if (maxLines <= 0)
            return new SplitRenderResult { HeightUsed = 0, RenderedPart = null, Remainder = (ParagraphElement)element.Clone() };

        if (maxLines > lines.Count)
            maxLines = lines.Count;

        var renderedLines = lines.Take(maxLines).ToList();
        var remainingLines = lines.Skip(maxLines).ToList();

        RenderLines(gfx, renderedLines, font, brush, style, x, y, width, lineHeight, spaceBefore);

        return new SplitRenderResult
        {
            HeightUsed = spaceBefore + renderedLines.Count * lineHeight,
            RenderedPart = element.CloneWithContent(string.Join("\n", renderedLines)),
            Remainder = remainingLines.Count > 0 ? element.CloneWithContent(string.Join("\n", remainingLines)) : null
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
}
