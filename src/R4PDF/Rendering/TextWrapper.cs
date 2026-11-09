using PdfSharpCore.Drawing;

namespace R4PDF.Rendering;

/// <summary>
///     Word wrapping of paragraphs and table cells. Lines break between words; a word that is wider
///     than a line on its own (a file name, an identifier, a URL) is broken inside, after a separator
///     such as "_", "-" or "/" when there is one, so it never runs over the edge of its column.
/// </summary>
internal static class TextWrapper
{
    private static readonly char[] BreakAfter = { '_', '-', '/', '\\', '.', ',', ';', ':', '|', '@' };

    /// <summary>
    ///     The lines of <paramref name="text" /> within <paramref name="maxWidth" />. A newline starts a new
    ///     line and an empty paragraph gives an empty line; empty text gives no lines.
    /// </summary>
    internal static List<string> Wrap(XGraphics gfx, string? text, XFont font, double maxWidth)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text))
            return lines;

        foreach (var paragraph in text.Replace("\r\n", "\n").Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(paragraph))
            {
                lines.Add(string.Empty);
                continue;
            }

            WrapParagraph(gfx, paragraph, font, maxWidth, lines);
        }

        return lines;
    }

    private static void WrapParagraph(XGraphics gfx, string paragraph, XFont font, double maxWidth, List<string> lines)
    {
        var currentLine = string.Empty;

        foreach (var word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var testLine = currentLine.Length == 0 ? word : $"{currentLine} {word}";
            if (Fits(gfx, testLine, font, maxWidth))
            {
                currentLine = testLine;
                continue;
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine);

            currentLine = BreakWord(gfx, word, font, maxWidth, lines);
        }

        if (currentLine.Length > 0)
            lines.Add(currentLine);
    }

    /// <summary>
    ///     Adds the full lines of a word that does not fit a line on its own and returns the rest, which
    ///     the next word may still join.
    /// </summary>
    private static string BreakWord(XGraphics gfx, string word, XFont font, double maxWidth, List<string> lines)
    {
        var rest = word;
        while (!Fits(gfx, rest, font, maxWidth))
        {
            var length = FittingLength(gfx, rest, font, maxWidth);

            // Break after a separator in the second half of the piece; earlier would waste the line.
            var separator = rest.LastIndexOfAny(BreakAfter, length - 1);
            if (separator >= length / 2 && separator < rest.Length - 1)
                length = separator + 1;

            lines.Add(rest[..length]);
            rest = rest[length..];
        }

        return rest;
    }

    /// <summary>The number of leading characters that fit, at least one so wrapping always progresses.</summary>
    private static int FittingLength(XGraphics gfx, string text, XFont font, double maxWidth)
    {
        int low = 1, high = text.Length;
        while (low < high)
        {
            var middle = (low + high + 1) / 2;
            if (Fits(gfx, text[..middle], font, maxWidth))
                low = middle;
            else
                high = middle - 1;
        }

        return low;
    }

    private static bool Fits(XGraphics gfx, string text, XFont font, double maxWidth)
    {
        return gfx.MeasureString(text, font).Width <= maxWidth;
    }
}
