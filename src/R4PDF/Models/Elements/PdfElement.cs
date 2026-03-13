using System.Text.Json.Serialization;

namespace R4PDF.Models.Elements;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextElement), "text")]
[JsonDerivedType(typeof(ParagraphElement), "paragraph")]
[JsonDerivedType(typeof(TableElement), "table")]
[JsonDerivedType(typeof(ImageElement), "image")]
[JsonDerivedType(typeof(LineElement), "line")]
[JsonDerivedType(typeof(RectangleElement), "rectangle")]
public abstract class PdfElement
{
    public string? Style { get; set; }
    public PdfStyle? InlineStyle { get; set; }
    public string? X { get; set; }
    public string? Y { get; set; }
    public string? Width { get; set; }
    public string? Height { get; set; }
}
