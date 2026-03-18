using R4PDF.Fluent.Themes;
using R4PDF.Models;

namespace R4PDF.Fluent.Builders;

/// <summary>
/// Builder for a single PDF page. Supports header, body, and footer sections.
/// </summary>
public class PageBuilder
{
    private readonly PdfTheme? _theme;
    private SectionDefinition? _header;
    private SectionDefinition _body = new();
    private SectionDefinition? _footer;
    private PageSettings? _settings;

    internal PageBuilder(PdfTheme? theme)
    {
        _theme = theme;
    }

    /// <summary>
    /// Defines the header section. Theme header defaults (height, text style) are applied automatically.
    /// </summary>
    public PageBuilder Header(Action<SectionBuilder> configure)
    {
        var builder = new SectionBuilder(_theme);

        // Apply theme section defaults
        if (_theme?.Header.Height != null)
            builder.Height(_theme.Header.Height);
        if (_theme?.Header.Background != null)
            builder.Background(_theme.Header.Background);

        configure(builder);
        _header = builder.Build();
        return this;
    }

    /// <summary>
    /// Defines the body section (required — this is where main content goes).
    /// </summary>
    public PageBuilder Body(Action<SectionBuilder> configure)
    {
        var builder = new SectionBuilder(_theme);
        configure(builder);
        _body = builder.Build();
        return this;
    }

    /// <summary>
    /// Defines the footer section. Theme footer defaults (height, text style) are applied automatically.
    /// </summary>
    public PageBuilder Footer(Action<SectionBuilder> configure)
    {
        var builder = new SectionBuilder(_theme);

        // Apply theme section defaults
        if (_theme?.Footer.Height != null)
            builder.Height(_theme.Footer.Height);
        if (_theme?.Footer.Background != null)
            builder.Background(_theme.Footer.Background);

        configure(builder);
        _footer = builder.Build();
        return this;
    }

    /// <summary>
    /// Overrides page settings for this specific page.
    /// </summary>
    public PageBuilder Settings(Action<SettingsBuilder> configure)
    {
        var builder = new SettingsBuilder();
        configure(builder);
        _settings = builder.Settings;
        return this;
    }

    internal PageDefinition Build() => new()
    {
        Settings = _settings,
        Header = _header,
        Body = _body,
        Footer = _footer
    };
}
