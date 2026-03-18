using R4PDF.Fluent.Themes;
using R4PDF.Models;

namespace R4PDF.Fluent.Builders;

/// <summary>
///     Top-level fluent builder for constructing a PDF document.
///     Use <see cref="Pdf.Create" /> to get an instance.
/// </summary>
/// <example>
///     Pdf.Create()
///     .WithTheme(PdfTheme.Default)
///     .WithMetadata(m => m.Title("My Report").Author("Team"))
///     .AddPage(page => page
///     .Header(h => h.Text("Report Header"))
///     .Body(b => b
///     .Heading1("Introduction")
///     .Paragraph("This is the content...")
///     .Table(t => t
///     .Column("Name", "50%")
///     .Column("Value", "50%")
///     .Row("Item 1", "100")
///     .Row("Item 2", "200")))
///     .Footer(f => f.Text("Page {pageNumber} of {pageCount}")))
///     .GenerateToFile("report.pdf");
/// </example>
public class PdfDocumentBuilder
{
    private readonly Dictionary<string, PdfStyle> _customStyles = new();
    private readonly List<PageDefinition> _pages = new();
    private DocumentMetadata? _metadata;
    private PageSettings? _settings;
    private PdfTheme? _theme;

    internal PdfDocumentBuilder()
    {
    }

    /// <summary>
    ///     Sets the visual theme for the document. All components will use this theme's defaults.
    /// </summary>
    public PdfDocumentBuilder WithTheme(PdfTheme theme)
    {
        _theme = theme;
        return this;
    }

    /// <summary>
    ///     Configures document metadata (title, author, subject, keywords).
    /// </summary>
    public PdfDocumentBuilder WithMetadata(Action<MetadataBuilder> configure)
    {
        var builder = new MetadataBuilder();
        configure(builder);
        _metadata = builder.Metadata;
        return this;
    }

    /// <summary>
    ///     Overrides page settings for the entire document. Takes priority over theme page settings.
    /// </summary>
    public PdfDocumentBuilder WithSettings(Action<SettingsBuilder> configure)
    {
        var builder = new SettingsBuilder(_theme?.PageSettings);
        configure(builder);
        _settings = builder.Settings;
        return this;
    }

    /// <summary>
    ///     Adds a custom named style. These are merged with theme styles (custom styles take priority).
    /// </summary>
    public PdfDocumentBuilder AddStyle(string name, PdfStyle style)
    {
        _customStyles[name] = style;
        return this;
    }

    /// <summary>
    ///     Adds a page to the document.
    /// </summary>
    public PdfDocumentBuilder AddPage(Action<PageBuilder> configure)
    {
        var builder = new PageBuilder(_theme);
        configure(builder);
        _pages.Add(builder.Build());
        return this;
    }

    /// <summary>
    ///     Builds the PdfTemplate model. Use this to inspect the template or pass to PdfGenerator directly.
    /// </summary>
    public PdfTemplate Build()
    {
        // Merge theme styles + custom styles (custom overrides theme)
        var styles = _theme?.ToStylesDictionary() ?? new Dictionary<string, PdfStyle>();
        foreach (var kvp in _customStyles)
            styles[kvp.Key] = kvp.Value;

        // Page settings: explicit > theme > defaults
        var settings = _settings ?? _theme?.PageSettings ?? new PageSettings();

        return new PdfTemplate
        {
            Metadata = _metadata,
            Settings = settings,
            Styles = styles,
            Pages = _pages
        };
    }

    /// <summary>
    ///     Builds the template and generates a PDF as a byte array.
    /// </summary>
    public byte[] Generate()
    {
        var template = Build();
        var generator = new PdfGenerator();
        return generator.Generate(template);
    }

    /// <summary>
    ///     Builds the template and writes the PDF to a stream.
    /// </summary>
    public void GenerateToStream(Stream outputStream)
    {
        var template = Build();
        var generator = new PdfGenerator();
        generator.GenerateToStream(template, outputStream);
    }

    /// <summary>
    ///     Builds the template and saves the PDF to a file.
    /// </summary>
    public void GenerateToFile(string outputPath)
    {
        var template = Build();
        var generator = new PdfGenerator();
        generator.GenerateToFile(template, outputPath);
    }
}