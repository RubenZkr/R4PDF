using R4PDF.Exceptions;
using R4PDF.Models;
using R4PDF.Parsing;
using R4PDF.Rendering;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace R4PDF;

/// <summary>
/// Converts JSON templates into PDF documents.
/// </summary>
public class PdfGenerator
{
    public PdfGenerator()
    {
        SystemFontResolver.EnsureRegistered();
    }

    /// <summary>
    /// Generates a PDF from a JSON template and returns it as a byte array.
    /// </summary>
    /// <param name="templateJson">JSON string defining the PDF layout and content.</param>
    /// <param name="dataJson">Optional JSON string with data for placeholder binding (${path.to.value}).</param>
    /// <returns>PDF file as a byte array.</returns>
    public byte[] Generate(string templateJson, string? dataJson = null)
    {
        using var stream = new MemoryStream();
        GenerateToStream(templateJson, dataJson, stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Generates a PDF from a JSON template and writes it to a stream.
    /// </summary>
    public void GenerateToStream(string templateJson, string? dataJson, Stream outputStream)
    {
        try
        {
            // Phase 1: Bind data placeholders
            var boundJson = DataBinder.Bind(templateJson, dataJson);

            // Phase 2: Parse template
            var template = TemplateParser.Parse(boundJson);

            // Phase 3: Render to PDF
            RenderTemplate(template, outputStream);
        }
        catch (PdfGenerationException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not PdfGenerationException)
        {
            throw new PdfGenerationException($"Failed to generate PDF: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generates a PDF from a PdfTemplate model and returns it as a byte array.
    /// Use this with the fluent builder API.
    /// </summary>
    public byte[] Generate(PdfTemplate template)
    {
        using var stream = new MemoryStream();
        GenerateToStream(template, stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Generates a PDF from a PdfTemplate model and writes it to a stream.
    /// </summary>
    public void GenerateToStream(PdfTemplate template, Stream outputStream)
    {
        try
        {
            RenderTemplate(template, outputStream);
        }
        catch (PdfGenerationException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not PdfGenerationException)
        {
            throw new PdfGenerationException($"Failed to generate PDF: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generates a PDF from a PdfTemplate model and saves it to a file.
    /// </summary>
    public void GenerateToFile(PdfTemplate template, string outputPath)
    {
        var bytes = Generate(template);
        File.WriteAllBytes(outputPath, bytes);
    }

    /// <summary>
    /// Generates a PDF from a JSON template and saves it to a file.
    /// </summary>
    public void GenerateToFile(string templateJson, string outputPath)
    {
        GenerateToFile(templateJson, null, outputPath);
    }

    /// <summary>
    /// Generates a PDF from a JSON template with data binding and saves it to a file.
    /// </summary>
    public void GenerateToFile(string templateJson, string? dataJson, string outputPath)
    {
        var bytes = Generate(templateJson, dataJson);
        File.WriteAllBytes(outputPath, bytes);
    }

    private void RenderTemplate(PdfTemplate template, Stream outputStream)
    {
        using var document = CreatePdfDocument(template);
        var styleResolver = new StyleResolver(template.Styles);
        var pageRenderer = new PageRenderer(styleResolver);

        int pageCount = template.Pages.Count;

        for (int i = 0; i < template.Pages.Count; i++)
        {
            var pageDefinition = template.Pages[i];
            var settings = pageDefinition.Settings ?? template.Settings;

            var pdfPage = document.AddPage();
            ConfigurePage(pdfPage, settings);

            using var gfx = XGraphics.FromPdfPage(pdfPage);
            pageRenderer.Render(gfx, pageDefinition, template.Settings, i + 1, pageCount);
        }

        document.Save(outputStream);
    }

    private static PdfDocument CreatePdfDocument(PdfTemplate template)
    {
        var document = new PdfDocument();

        if (template.Metadata != null)
        {
            document.Info.Title = template.Metadata.Title ?? "";
            document.Info.Author = template.Metadata.Author ?? "";
            document.Info.Subject = template.Metadata.Subject ?? "";
            document.Info.Keywords = template.Metadata.Keywords ?? "";
        }

        return document;
    }

    private static void ConfigurePage(PdfPage page, PageSettings settings)
    {
        page.Size = settings.PageSize?.ToUpperInvariant() switch
        {
            "A4" => PdfSharpCore.PageSize.A4,
            "A3" => PdfSharpCore.PageSize.A3,
            "A5" => PdfSharpCore.PageSize.A5,
            "LETTER" => PdfSharpCore.PageSize.Letter,
            "LEGAL" => PdfSharpCore.PageSize.Legal,
            "TABLOID" => PdfSharpCore.PageSize.Tabloid,
            _ => PdfSharpCore.PageSize.A4,
        };

        page.Orientation = settings.Orientation?.ToUpperInvariant() switch
        {
            "LANDSCAPE" => PdfSharpCore.PageOrientation.Landscape,
            _ => PdfSharpCore.PageOrientation.Portrait,
        };
    }
}
