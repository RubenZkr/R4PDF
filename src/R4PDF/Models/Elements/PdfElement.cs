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
}
