using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.Annotations;
using R4PDF.Models;

namespace R4PDF.Rendering;

/// <summary>
///     Collects the anchors and links of the final render pass; the layout passes of auto-pagination
///     render too, but without a recorder. Links are added once every page is rendered, because a link
///     can point to an anchor on a later page.
/// </summary>
internal sealed class LayoutRecorder
{
    private readonly List<PdfAnchor> _anchors = new();
    private readonly List<(int PageIndex, XRect Area, string Target)> _links = new();

    /// <summary>The page that is being rendered.</summary>
    internal int PageIndex { get; set; }

    internal IReadOnlyList<PdfAnchor> Anchors => _anchors;

    internal void Record(string? anchor, string? link, XRect area)
    {
        if (!string.IsNullOrEmpty(anchor))
            _anchors.Add(new PdfAnchor(anchor, PageIndex, area.X, area.Y, area.Width, area.Height));

        if (!string.IsNullOrEmpty(link))
            _links.Add((PageIndex, area, link));
    }

    /// <summary>Adds the links to the pages; a link to an anchor that does not exist is left out.</summary>
    internal void AddLinks(PdfDocument document)
    {
        foreach (var (pageIndex, area, target) in _links)
        {
            var page = document.Pages[pageIndex];
            var rectangle = ToPdfRectangle(page, area);

            if (!target.StartsWith('#'))
            {
                page.AddWebLink(rectangle, target);
                continue;
            }

            var anchor = _anchors.FirstOrDefault(item => item.Name == target[1..]);
            if (anchor != null)
                page.AddDocumentLink(rectangle, anchor.PageIndex + 1);
        }
    }

    /// <summary>Annotations use the PDF coordinate system, which starts at the bottom of the page.</summary>
    internal static PdfRectangle ToPdfRectangle(PdfPage page, XRect area)
    {
        var height = page.Height.Point;
        return new PdfRectangle(new XPoint(area.Left, height - area.Bottom), new XPoint(area.Right, height - area.Top));
    }
}
