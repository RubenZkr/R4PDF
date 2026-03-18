using R4PDF.Models;

namespace R4PDF.Fluent.Themes;

/// <summary>
///     Theme settings for header/footer sections.
/// </summary>
public class SectionTheme
{
    public string? Height { get; set; }
    public string? Background { get; set; }
    public PdfStyle TextStyle { get; set; } = new();

    public SectionTheme Clone()
    {
        return new SectionTheme
        {
            Height = Height,
            Background = Background,
            TextStyle = CloneStyle(TextStyle)
        };
    }

    internal static PdfStyle CloneStyle(PdfStyle source)
    {
        return new PdfStyle
        {
            FontFamily = source.FontFamily,
            FontSize = source.FontSize,
            FontWeight = source.FontWeight,
            FontStyle = source.FontStyle,
            Color = source.Color,
            BackgroundColor = source.BackgroundColor,
            Alignment = source.Alignment,
            LineHeight = source.LineHeight,
            Padding = source.Padding,
            Border = source.Border is null
                ? null
                : new BorderStyle
                {
                    Width = source.Border.Width,
                    Color = source.Border.Color,
                    Type = source.Border.Type
                }
        };
    }
}