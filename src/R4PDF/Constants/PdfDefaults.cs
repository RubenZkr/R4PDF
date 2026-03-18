namespace R4PDF;

/// <summary>
///     Default values used throughout the PDF generation pipeline.
///     These are applied when no explicit value is provided.
/// </summary>
public static class PdfDefaults
{
    // Page
    public const string PageSize = PageSizes.A4;
    public const string Orientation = Orientations.Portrait;

    // Margins
    public const string MarginTop = "20mm";
    public const string MarginBottom = "20mm";
    public const string MarginLeft = "15mm";
    public const string MarginRight = "15mm";

    // Font / style
    public const string FontFamily = FontFamilies.LiberationSans;
    public const double FontSize = 12;
    public const string FontWeight = FontWeights.Normal;
    public const string FontStyle = FontStyles.Normal;
    public const string Alignment = Alignments.Left;
    public const double LineHeight = 1.2;

    // Border
    public const string BorderType = BorderTypes.All;
    public const double BorderWidth = 0.5;

    // Table
    public const double TableCellPadding = 4;
    public const double TableHeaderFontSize = 12;
    public const double TableDataFontSize = 11;

    // Rendering
    public const double DefaultRectangleHeight = 20;
    public const double DefaultStrokeWidth = 1.0;
    public const double DefaultFooterHeight = 30;
}