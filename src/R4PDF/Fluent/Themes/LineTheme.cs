namespace R4PDF.Fluent.Themes;

/// <summary>
///     Theme settings for line/divider elements.
/// </summary>
public class LineTheme
{
    public string Color { get; set; } = "#DEE2E6";
    public string StrokeWidth { get; set; } = "1pt";

    public LineTheme Clone()
    {
        return (LineTheme)MemberwiseClone();
    }
}