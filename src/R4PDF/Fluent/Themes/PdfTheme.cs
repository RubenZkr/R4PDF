using R4PDF.Models;

namespace R4PDF.Fluent.Themes;

/// <summary>
/// Defines a complete visual theme for PDF generation.
/// Themes provide default styles for all component types, which are applied automatically
/// when using the fluent API. Individual elements can still override theme defaults.
/// </summary>
public class PdfTheme
{
    // ── Page defaults ────────────────────────────────────────────────────
    public PageSettings PageSettings { get; set; } = new();

    // ── Text styles ──────────────────────────────────────────────────────
    public PdfStyle Text { get; set; } = new();
    public PdfStyle Heading1 { get; set; } = new();
    public PdfStyle Heading2 { get; set; } = new();
    public PdfStyle Heading3 { get; set; } = new();
    public PdfStyle Accent { get; set; } = new();
    public PdfStyle Muted { get; set; } = new();
    public PdfStyle Caption { get; set; } = new();

    // ── Paragraph ────────────────────────────────────────────────────────
    public PdfStyle Paragraph { get; set; } = new();

    // ── Component themes ─────────────────────────────────────────────────
    public TableTheme Table { get; set; } = new();
    public LineTheme Line { get; set; } = new();
    public SectionTheme Header { get; set; } = new();
    public SectionTheme Footer { get; set; } = new();

    // ── Built-in themes ──────────────────────────────────────────────────

    /// <summary>Clean professional light theme with blues and grays.</summary>
    public static PdfTheme Default => BuiltInThemes.CreateDefault();

    /// <summary>Dark background theme with light text.</summary>
    public static PdfTheme Dark => BuiltInThemes.CreateDark();

    /// <summary>Minimalist theme with larger spacing and modern typography.</summary>
    public static PdfTheme Modern => BuiltInThemes.CreateModern();

    // ── Style dictionary conversion ──────────────────────────────────────

    /// <summary>
    /// Converts this theme's named styles into a dictionary compatible with PdfTemplate.Styles.
    /// The StyleResolver uses these keys to apply styles to elements that reference them by name.
    /// </summary>
    public Dictionary<string, PdfStyle> ToStylesDictionary()
    {
        var styles = new Dictionary<string, PdfStyle>();

        AddIfConfigured(styles, ThemeStyleNames.Text, Text);
        AddIfConfigured(styles, ThemeStyleNames.Heading1, Heading1);
        AddIfConfigured(styles, ThemeStyleNames.Heading2, Heading2);
        AddIfConfigured(styles, ThemeStyleNames.Heading3, Heading3);
        AddIfConfigured(styles, ThemeStyleNames.Accent, Accent);
        AddIfConfigured(styles, ThemeStyleNames.Muted, Muted);
        AddIfConfigured(styles, ThemeStyleNames.Caption, Caption);
        AddIfConfigured(styles, ThemeStyleNames.Paragraph, Paragraph);

        return styles;
    }

    /// <summary>Creates a deep clone of this theme.</summary>
    public PdfTheme Clone() => new()
    {
        PageSettings = new PageSettings
        {
            PageSize = PageSettings.PageSize,
            Orientation = PageSettings.Orientation,
            Margins = new MarginSettings
            {
                Top = PageSettings.Margins.Top,
                Bottom = PageSettings.Margins.Bottom,
                Left = PageSettings.Margins.Left,
                Right = PageSettings.Margins.Right
            }
        },
        Text = SectionTheme.CloneStyle(Text),
        Heading1 = SectionTheme.CloneStyle(Heading1),
        Heading2 = SectionTheme.CloneStyle(Heading2),
        Heading3 = SectionTheme.CloneStyle(Heading3),
        Accent = SectionTheme.CloneStyle(Accent),
        Muted = SectionTheme.CloneStyle(Muted),
        Caption = SectionTheme.CloneStyle(Caption),
        Paragraph = SectionTheme.CloneStyle(Paragraph),
        Table = Table.Clone(),
        Line = Line.Clone(),
        Header = Header.Clone(),
        Footer = Footer.Clone()
    };

    private static void AddIfConfigured(Dictionary<string, PdfStyle> styles, string key, PdfStyle style)
    {
        if (style.FontFamily != null || style.FontSize.HasValue || style.FontWeight != null ||
            style.FontStyle != null || style.Color != null || style.BackgroundColor != null ||
            style.Alignment != null || style.LineHeight.HasValue)
        {
            styles[key] = style;
        }
    }
}

/// <summary>
/// Well-known style names used by the theme system and fluent builders.
/// </summary>
public static class ThemeStyleNames
{
    public const string Text = "_theme_text";
    public const string Heading1 = "_theme_h1";
    public const string Heading2 = "_theme_h2";
    public const string Heading3 = "_theme_h3";
    public const string Accent = "_theme_accent";
    public const string Muted = "_theme_muted";
    public const string Caption = "_theme_caption";
    public const string Paragraph = "_theme_paragraph";
}
