using R4PDF.Models;

namespace R4PDF.Fluent.Themes;

/// <summary>
/// Theme settings for table elements.
/// </summary>
public class TableTheme
{
    public string HeaderBackgroundColor { get; set; } = "#003366";
    public string HeaderTextColor { get; set; } = "#FFFFFF";
    public string HeaderFontWeight { get; set; } = FontWeights.Bold;
    public double HeaderFontSize { get; set; } = PdfDefaults.TableHeaderFontSize;
    public double DataFontSize { get; set; } = PdfDefaults.TableDataFontSize;
    public bool AlternateRowColors { get; set; } = true;
    public string AlternateColor { get; set; } = "#F8F9FA";
    public string BorderColor { get; set; } = "#DEE2E6";
    public string BorderWidth { get; set; } = "0.5pt";

    public TableTheme Clone() => (TableTheme)MemberwiseClone();
}

/// <summary>
/// Theme settings for line/divider elements.
/// </summary>
public class LineTheme
{
    public string Color { get; set; } = "#DEE2E6";
    public string StrokeWidth { get; set; } = "1pt";

    public LineTheme Clone() => (LineTheme)MemberwiseClone();
}

/// <summary>
/// Theme settings for header/footer sections.
/// </summary>
public class SectionTheme
{
    public string? Height { get; set; }
    public string? Background { get; set; }
    public PdfStyle TextStyle { get; set; } = new();

    public SectionTheme Clone() => new()
    {
        Height = Height,
        Background = Background,
        TextStyle = CloneStyle(TextStyle)
    };

    internal static PdfStyle CloneStyle(PdfStyle source) => new()
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
        Border = source.Border is null ? null : new BorderStyle
        {
            Width = source.Border.Width,
            Color = source.Border.Color,
            Type = source.Border.Type
        }
    };
}
