using System.Text.Json.Serialization;

namespace R4PDF.Models.Elements;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextElement), ElementTypes.Text)]
[JsonDerivedType(typeof(ParagraphElement), ElementTypes.Paragraph)]
[JsonDerivedType(typeof(TableElement), ElementTypes.Table)]
[JsonDerivedType(typeof(ImageElement), ElementTypes.Image)]
[JsonDerivedType(typeof(LineElement), ElementTypes.Line)]
[JsonDerivedType(typeof(RectangleElement), ElementTypes.Rectangle)]
[JsonDerivedType(typeof(CanvasElement), ElementTypes.Canvas)]
public abstract class PdfElement
{
    public string? Style { get; set; }
    public PdfStyle? InlineStyle { get; set; }
    public string? X { get; set; }
    public string? Y { get; set; }
    public string? Width { get; set; }
    public string? Height { get; set; }

    /// <summary>
    ///     With auto-pagination, keeps this element on the same page as the start of the next one (the
    ///     first line of a paragraph, the header and first row of a table): when they do not fit
    ///     together, this element moves to the next page. Meant for headings. A chain of elements
    ///     that all keep with next moves as a whole. On a page that holds nothing yet the element is
    ///     rendered regardless, so a chain taller than a page cannot loop.
    /// </summary>
    public bool KeepWithNext { get; set; }

    /// <summary>
    ///     Names the place where the element is rendered. <see cref="PdfGenerator.GenerateWithLayout(string, string?)" />
    ///     reports the page and area of every anchor, and a <see cref="Link" /> of "#name" jumps to it. A
    ///     paragraph or table that is split over pages reports each part.
    /// </summary>
    public string? Anchor { get; set; }

    /// <summary>
    ///     Makes the element clickable: "#name" jumps to the page of the anchor with that name, anything
    ///     else is opened as a URL.
    /// </summary>
    public string? Link { get; set; }

    /// <summary>
    ///     A copy that can be changed without changing this element, as auto-pagination does when it moves
    ///     or splits elements. Every property is copied, so a new property needs no extra code; elements
    ///     with lists override this to copy those too. Styles are shared: rendering never changes them.
    /// </summary>
    public virtual PdfElement Clone()
    {
        return (PdfElement)MemberwiseClone();
    }
}