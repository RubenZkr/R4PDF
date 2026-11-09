namespace R4PDF.Models;

/// <summary>
///     Where an element or table row with an anchor was rendered: the page (0-based) and its area in
///     points, measured from the top-left corner of the page.
/// </summary>
public sealed record PdfAnchor(string Name, int PageIndex, double X, double Y, double Width, double Height);

/// <summary>The PDF and the place of every anchor in it.</summary>
public sealed record PdfRenderResult(byte[] Pdf, IReadOnlyList<PdfAnchor> Anchors);
