namespace R4PDF.Fluent.Options;

/// <summary>
///     Inline override options for image elements.
/// </summary>
public class ImageOptions
{
    public string? Width { get; set; }
    public string? Height { get; set; }
    public string? Alignment { get; set; }
    public bool MaintainAspectRatio { get; set; } = true;
}