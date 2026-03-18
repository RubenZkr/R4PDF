namespace R4PDF;

/// <summary>
/// Predefined page sizes. Use these or pass any string supported by your PDF engine.
/// </summary>
public static class PageSizes
{
    public const string A3 = "A3";
    public const string A4 = "A4";
    public const string A5 = "A5";
    public const string Letter = "Letter";
    public const string Legal = "Legal";
    public const string Tabloid = "Tabloid";
}

/// <summary>
/// Predefined page orientations.
/// </summary>
public static class Orientations
{
    public const string Portrait = "Portrait";
    public const string Landscape = "Landscape";
}

/// <summary>
/// Predefined font weight values.
/// </summary>
public static class FontWeights
{
    public const string Normal = "normal";
    public const string Bold = "bold";
}

/// <summary>
/// Predefined font style values.
/// </summary>
public static class FontStyles
{
    public const string Normal = "normal";
    public const string Italic = "italic";
}

/// <summary>
/// Predefined text alignment values.
/// </summary>
public static class Alignments
{
    public const string Left = "left";
    public const string Center = "center";
    public const string Right = "right";
}

/// <summary>
/// Predefined element type discriminators used in JSON templates.
/// </summary>
public static class ElementTypes
{
    public const string Text = "text";
    public const string Paragraph = "paragraph";
    public const string Table = "table";
    public const string Image = "image";
    public const string Line = "line";
    public const string Rectangle = "rectangle";
}

/// <summary>
/// Predefined measurement unit suffixes.
/// </summary>
public static class Units
{
    public const string Points = "pt";
    public const string Millimeters = "mm";
    public const string Centimeters = "cm";
    public const string Inches = "in";
    public const string Pixels = "px";
}

/// <summary>
/// Predefined color names that can be used in style color properties.
/// Hex values (#RGB, #RRGGBB, #AARRGGBB) are also supported.
/// </summary>
public static class Colors
{
    public const string Black = "black";
    public const string White = "white";
    public const string Red = "red";
    public const string Green = "green";
    public const string Blue = "blue";
    public const string Gray = "gray";
    public const string Grey = "grey";
    public const string Yellow = "yellow";
    public const string Orange = "orange";
    public const string Purple = "purple";
    public const string DarkGray = "darkgray";
    public const string DarkGrey = "darkgrey";
    public const string LightGray = "lightgray";
    public const string LightGrey = "lightgrey";
    public const string Transparent = "transparent";
}

/// <summary>
/// Predefined border type values.
/// </summary>
public static class BorderTypes
{
    public const string All = "all";
    public const string Top = "top";
    public const string Bottom = "bottom";
    public const string Left = "left";
    public const string Right = "right";
}

/// <summary>
/// Predefined font family names recognized by the system font resolver.
/// </summary>
public static class FontFamilies
{
    public const string LiberationSans = "Liberation Sans";
    public const string Helvetica = "Helvetica";
    public const string Arial = "Arial";
    public const string SansSerif = "sans-serif";
    public const string Verdana = "Verdana";
    public const string Tahoma = "Tahoma";
    public const string SegoeUI = "Segoe UI";
    public const string Calibri = "Calibri";
}

/// <summary>
/// Predefined placeholder tokens for page numbering in text/paragraph elements.
/// </summary>
public static class Placeholders
{
    public const string PageNumber = "{pageNumber}";
    public const string PageCount = "{pageCount}";
}

/// <summary>
/// Default values used throughout the PDF generation pipeline.
/// These are applied when no explicit value is provided.
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
