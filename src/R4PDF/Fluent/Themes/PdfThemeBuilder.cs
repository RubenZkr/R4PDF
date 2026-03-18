using R4PDF.Models;

namespace R4PDF.Fluent.Themes;

/// <summary>
///     Builds a custom PdfTheme by starting from a base theme and overriding individual components.
/// </summary>
/// <example>
///     var theme = new PdfThemeBuilder(PdfTheme.Default)
///     .Heading1(s => s.Color = "#FF0000")
///     .TableHeader(t => t.HeaderBackgroundColor = "#222222")
///     .Build();
/// </example>
public class PdfThemeBuilder
{
    private readonly PdfTheme _theme;

    public PdfThemeBuilder() : this(PdfTheme.Default)
    {
    }

    public PdfThemeBuilder(PdfTheme baseTheme)
    {
        _theme = baseTheme.Clone();
    }

    public PdfThemeBuilder PageSettings(Action<PageSettings> configure)
    {
        configure(_theme.PageSettings);
        return this;
    }

    public PdfThemeBuilder Text(Action<PdfStyle> configure)
    {
        configure(_theme.Text);
        return this;
    }

    public PdfThemeBuilder Heading1(Action<PdfStyle> configure)
    {
        configure(_theme.Heading1);
        return this;
    }

    public PdfThemeBuilder Heading2(Action<PdfStyle> configure)
    {
        configure(_theme.Heading2);
        return this;
    }

    public PdfThemeBuilder Heading3(Action<PdfStyle> configure)
    {
        configure(_theme.Heading3);
        return this;
    }

    public PdfThemeBuilder Accent(Action<PdfStyle> configure)
    {
        configure(_theme.Accent);
        return this;
    }

    public PdfThemeBuilder Muted(Action<PdfStyle> configure)
    {
        configure(_theme.Muted);
        return this;
    }

    public PdfThemeBuilder Caption(Action<PdfStyle> configure)
    {
        configure(_theme.Caption);
        return this;
    }

    public PdfThemeBuilder Paragraph(Action<PdfStyle> configure)
    {
        configure(_theme.Paragraph);
        return this;
    }

    public PdfThemeBuilder Table(Action<TableTheme> configure)
    {
        configure(_theme.Table);
        return this;
    }

    public PdfThemeBuilder Line(Action<LineTheme> configure)
    {
        configure(_theme.Line);
        return this;
    }

    public PdfThemeBuilder Header(Action<SectionTheme> configure)
    {
        configure(_theme.Header);
        return this;
    }

    public PdfThemeBuilder Footer(Action<SectionTheme> configure)
    {
        configure(_theme.Footer);
        return this;
    }

    public PdfTheme Build()
    {
        return _theme;
    }
}