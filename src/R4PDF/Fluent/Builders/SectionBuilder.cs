using R4PDF.Fluent.Options;
using R4PDF.Fluent.Themes;
using R4PDF.Models;
using R4PDF.Models.Elements;

namespace R4PDF.Fluent.Builders;

/// <summary>
///     Builder for a content section (header, body, or footer).
///     Provides convenience methods for adding elements with automatic theme styling.
/// </summary>
public class SectionBuilder
{
    private readonly List<PdfElement> _elements = new();
    private readonly PdfTheme? _theme;
    private string? _background;
    private string? _height;

    internal SectionBuilder(PdfTheme? theme)
    {
        _theme = theme;
    }

    public SectionBuilder Height(string height)
    {
        _height = height;
        return this;
    }

    public SectionBuilder Background(string color)
    {
        _background = color;
        return this;
    }

    // ── Text ─────────────────────────────────────────────────────────────

    /// <summary>Adds a text element using the theme's text style.</summary>
    public SectionBuilder Text(string text)
    {
        _elements.Add(new TextElement
        {
            Text = text,
            Style = ThemeStyleNames.Text
        });
        return this;
    }

    /// <summary>Adds a text element with inline overrides.</summary>
    public SectionBuilder Text(string text, Action<TextOptions> configure)
    {
        var opts = new TextOptions();
        configure(opts);

        _elements.Add(new TextElement
        {
            Text = text,
            Style = opts.Style ?? ThemeStyleNames.Text,
            FontFamily = opts.FontFamily,
            FontSize = opts.FontSize,
            FontWeight = opts.FontWeight,
            Color = opts.Color,
            Alignment = opts.Alignment
        });
        return this;
    }

    // ── Headings ─────────────────────────────────────────────────────────

    public SectionBuilder Heading1(string text)
    {
        _elements.Add(new TextElement { Text = text, Style = ThemeStyleNames.Heading1 });
        return this;
    }

    public SectionBuilder Heading2(string text)
    {
        _elements.Add(new TextElement { Text = text, Style = ThemeStyleNames.Heading2 });
        return this;
    }

    public SectionBuilder Heading3(string text)
    {
        _elements.Add(new TextElement { Text = text, Style = ThemeStyleNames.Heading3 });
        return this;
    }

    // ── Accent / Muted / Caption ─────────────────────────────────────────

    public SectionBuilder AccentText(string text)
    {
        _elements.Add(new TextElement { Text = text, Style = ThemeStyleNames.Accent });
        return this;
    }

    public SectionBuilder MutedText(string text)
    {
        _elements.Add(new TextElement { Text = text, Style = ThemeStyleNames.Muted });
        return this;
    }

    public SectionBuilder CaptionText(string text)
    {
        _elements.Add(new TextElement { Text = text, Style = ThemeStyleNames.Caption });
        return this;
    }

    // ── Paragraph ────────────────────────────────────────────────────────

    public SectionBuilder Paragraph(string content)
    {
        _elements.Add(new ParagraphElement
        {
            Content = content,
            Style = ThemeStyleNames.Paragraph
        });
        return this;
    }

    public SectionBuilder Paragraph(string content, Action<ParagraphOptions> configure)
    {
        var opts = new ParagraphOptions();
        configure(opts);

        _elements.Add(new ParagraphElement
        {
            Content = content,
            Style = opts.Style ?? ThemeStyleNames.Paragraph,
            FontFamily = opts.FontFamily,
            FontSize = opts.FontSize,
            FontWeight = opts.FontWeight,
            Color = opts.Color,
            Alignment = opts.Alignment,
            LineHeight = opts.LineHeight
        });
        return this;
    }

    // ── Table ────────────────────────────────────────────────────────────

    public SectionBuilder Table(Action<TableBuilder> configure)
    {
        var builder = new TableBuilder(_theme?.Table);
        configure(builder);
        _elements.Add(builder.Build());
        return this;
    }

    // ── Line ─────────────────────────────────────────────────────────────

    /// <summary>Adds a horizontal line using theme defaults.</summary>
    public SectionBuilder Line()
    {
        _elements.Add(new LineElement
        {
            Color = _theme?.Line.Color,
            StrokeWidth = _theme?.Line.StrokeWidth
        });
        return this;
    }

    public SectionBuilder Line(Action<LineOptions> configure)
    {
        var opts = new LineOptions();
        configure(opts);

        _elements.Add(new LineElement
        {
            Color = opts.Color ?? _theme?.Line.Color,
            StrokeWidth = opts.StrokeWidth ?? _theme?.Line.StrokeWidth,
            DashPattern = opts.DashPattern
        });
        return this;
    }

    // ── Rectangle ────────────────────────────────────────────────────────

    public SectionBuilder Rectangle(Action<RectangleOptions> configure)
    {
        var opts = new RectangleOptions();
        configure(opts);

        _elements.Add(new RectangleElement
        {
            FillColor = opts.FillColor,
            StrokeColor = opts.StrokeColor,
            StrokeWidth = opts.StrokeWidth,
            Width = opts.Width,
            Height = opts.Height,
            CornerRadius = opts.CornerRadius
        });
        return this;
    }

    // ── Image ────────────────────────────────────────────────────────────

    public SectionBuilder Image(string source)
    {
        _elements.Add(new ImageElement { Source = source });
        return this;
    }

    public SectionBuilder Image(string source, Action<ImageOptions> configure)
    {
        var opts = new ImageOptions();
        configure(opts);

        _elements.Add(new ImageElement
        {
            Source = source,
            Width = opts.Width,
            Height = opts.Height,
            Alignment = opts.Alignment,
            MaintainAspectRatio = opts.MaintainAspectRatio
        });
        return this;
    }

    // ── Spacer ───────────────────────────────────────────────────────────

    /// <summary>Adds an empty text element for vertical spacing.</summary>
    public SectionBuilder Spacer()
    {
        _elements.Add(new TextElement { Text = "" });
        return this;
    }

    // ── Raw element ──────────────────────────────────────────────────────

    /// <summary>Adds a raw PdfElement directly for advanced scenarios.</summary>
    public SectionBuilder Element(PdfElement element)
    {
        _elements.Add(element);
        return this;
    }

    // ── Build ────────────────────────────────────────────────────────────

    internal SectionDefinition Build()
    {
        return new SectionDefinition
        {
            Height = _height,
            Background = _background,
            Elements = _elements
        };
    }
}