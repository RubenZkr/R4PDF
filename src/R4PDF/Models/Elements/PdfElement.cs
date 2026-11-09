using System.Text.Json.Serialization;

namespace R4PDF.Models.Elements;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextElement), ElementTypes.Text)]
[JsonDerivedType(typeof(ParagraphElement), ElementTypes.Paragraph)]
[JsonDerivedType(typeof(TableElement), ElementTypes.Table)]
[JsonDerivedType(typeof(ImageElement), ElementTypes.Image)]
[JsonDerivedType(typeof(LineElement), ElementTypes.Line)]
[JsonDerivedType(typeof(RectangleElement), ElementTypes.Rectangle)]
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
}