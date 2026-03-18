using R4PDF.Fluent.Builders;

namespace R4PDF.Fluent;

/// <summary>
/// Static entry point for the fluent PDF builder API.
/// </summary>
/// <example>
/// var pdf = Pdf.Create()
///     .WithTheme(PdfTheme.Default)
///     .AddPage(page => page.Body(b => b.Heading1("Hello!")))
///     .Generate();
/// </example>
public static class Pdf
{
    /// <summary>
    /// Creates a new PDF document builder.
    /// </summary>
    public static PdfDocumentBuilder Create() => new();
}
