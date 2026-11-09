namespace R4PDF.Models.Elements;

/// <summary>
///     A drawing area in the flow of the page, for diagrams and other free layouts. Its elements are
///     positioned with X and Y (and a line with X1, Y1, X2 and Y2) relative to the top-left corner of the
///     canvas, and do not move the layout cursor: only the canvas takes up room, its
///     <see cref="PdfElement.Height" /> or, without one, down to the lowest of its elements. With
///     auto-pagination the canvas moves to the next page as a whole when it does not fit.
/// </summary>
public class CanvasElement : PdfElement
{
    public List<PdfElement> Elements { get; set; } = new();

    public override PdfElement Clone()
    {
        var clone = (CanvasElement)base.Clone();
        clone.Elements = Elements.Select(element => element.Clone()).ToList();
        return clone;
    }
}
