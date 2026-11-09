using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using R4PDF.Exceptions;
using R4PDF.Models;
using R4PDF.Parsing;
using R4PDF.Rendering;

namespace R4PDF;

/// <summary>
///     Converts JSON templates into PDF documents.
/// </summary>
public class PdfGenerator
{
    public PdfGenerator()
    {
        SystemFontResolver.EnsureRegistered();
    }

    /// <summary>
    ///     Generates a PDF from a JSON template and returns it as a byte array.
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
    ///     Generates a PDF from a JSON template and reports where every element or table row with an
    ///     <c>anchor</c> was rendered, for example to link to it from another document.
    /// </summary>
    public PdfRenderResult GenerateWithLayout(string templateJson, string? dataJson = null)
    {
        using var stream = new MemoryStream();
        var anchors = Generate(() => RenderTemplate(TemplateParser.Parse(DataBinder.Bind(templateJson, dataJson)), stream));
        return new PdfRenderResult(stream.ToArray(), anchors);
    }

    /// <summary>
    ///     Generates a PDF from a PdfTemplate model and reports where every element or table row with an
    ///     anchor was rendered.
    /// </summary>
    public PdfRenderResult GenerateWithLayout(PdfTemplate template)
    {
        using var stream = new MemoryStream();
        var anchors = Generate(() => RenderTemplate(template, stream));
        return new PdfRenderResult(stream.ToArray(), anchors);
    }

    /// <summary>
    ///     Generates a PDF from a JSON template and writes it to a stream.
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
    ///     Generates a PDF from a PdfTemplate model and returns it as a byte array.
    ///     Use this with the fluent builder API.
    /// </summary>
    public byte[] Generate(PdfTemplate template)
    {
        using var stream = new MemoryStream();
        GenerateToStream(template, stream);
        return stream.ToArray();
    }

    /// <summary>
    ///     Generates a PDF from a PdfTemplate model and writes it to a stream.
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
    ///     Generates a PDF from a PdfTemplate model and saves it to a file.
    /// </summary>
    public void GenerateToFile(PdfTemplate template, string outputPath)
    {
        var bytes = Generate(template);
        File.WriteAllBytes(outputPath, bytes);
    }

    /// <summary>
    ///     Generates a PDF from a JSON template and saves it to a file.
    /// </summary>
    public void GenerateToFile(string templateJson, string outputPath)
    {
        GenerateToFile(templateJson, null, outputPath);
    }

    /// <summary>
    ///     Generates a PDF from a JSON template with data binding and saves it to a file.
    /// </summary>
    public void GenerateToFile(string templateJson, string? dataJson, string outputPath)
    {
        var bytes = Generate(templateJson, dataJson);
        File.WriteAllBytes(outputPath, bytes);
    }

    private static IReadOnlyList<PdfAnchor> Generate(Func<IReadOnlyList<PdfAnchor>> render)
    {
        try
        {
            return render();
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

    private IReadOnlyList<PdfAnchor> RenderTemplate(PdfTemplate template, Stream outputStream)
    {
        using var document = CreatePdfDocument(template);
        var styleResolver = new StyleResolver(template.Styles);
        var pageRenderer = new PageRenderer(styleResolver);

        var autoPagination = template.Settings.AutoPagination;
        var pages = autoPagination.Enabled
            ? MaterializeAutoPaginatedPages(template, pageRenderer)
            : template.Pages;

        var pageCount = pages.Count;
        var recorder = new LayoutRecorder();
        pageRenderer.Recorder = recorder;

        for (var i = 0; i < pages.Count; i++)
        {
            var pageDefinition = pages[i];
            var settings = pageDefinition.Settings ?? template.Settings;

            var pdfPage = document.AddPage();
            ConfigurePage(pdfPage, settings);

            recorder.PageIndex = i;
            using var gfx = XGraphics.FromPdfPage(pdfPage);
            pageRenderer.Render(gfx, pageDefinition, template.Settings, i + 1, pageCount);
        }

        recorder.AddLinks(document);
        document.Save(outputStream);
        return recorder.Anchors;
    }

    private static List<PageDefinition> MaterializeAutoPaginatedPages(PdfTemplate template, PageRenderer pageRenderer)
    {
        var materialized = new List<PageDefinition>();
        var workPages = template.Pages.Select(ClonePageDefinition).ToList();

        // Safety cap to avoid runaway loops if pagination cannot make progress.
        const int maxPages = 5000;

        for (var i = 0; i < workPages.Count; i++)
        {
            if (materialized.Count >= maxPages)
                throw new PdfGenerationException("Auto-pagination exceeded the maximum page limit (5000).");

            var sourcePage = workPages[i];
            var settings = sourcePage.Settings ?? template.Settings;
            var auto = settings.AutoPagination;

            using var layoutDocument = new PdfDocument();
            var layoutPage = layoutDocument.AddPage();
            ConfigurePage(layoutPage, settings);

            using var layoutGfx = XGraphics.FromPdfPage(layoutPage);
            var layout = pageRenderer.RenderWithAutoPagination(layoutGfx, sourcePage, template.Settings, 1, 1, auto);
            materialized.Add(layout.RenderedPage);

            if (layout.OverflowBody == null)
                continue;

            if (!layout.BodyConsumedAnyContent)
                throw new PdfGenerationException(
                    "Auto-pagination could not place any body content on a page. Check element sizes and margins.");

            var continuationPage = new PageDefinition
            {
                Settings = sourcePage.Settings,
                Header = auto.RepeatHeaderOnContinuation ? CloneSection(sourcePage.Header) : null,
                Body = layout.OverflowBody,
                Footer = auto.RepeatFooterOnContinuation ? CloneSection(sourcePage.Footer) : null
            };

            workPages.Insert(i + 1, continuationPage);
        }

        return materialized;
    }

    private static PageDefinition ClonePageDefinition(PageDefinition source)
    {
        return new PageDefinition
        {
            Settings = source.Settings == null
                ? null
                : new PageSettings
                {
                    PageSize = source.Settings.PageSize,
                    Orientation = source.Settings.Orientation,
                    Margins = new MarginSettings
                    {
                        Top = source.Settings.Margins.Top,
                        Bottom = source.Settings.Margins.Bottom,
                        Left = source.Settings.Margins.Left,
                        Right = source.Settings.Margins.Right
                    },
                    AutoPagination = new AutoPaginationSettings
                    {
                        Enabled = source.Settings.AutoPagination.Enabled,
                        RepeatHeaderOnContinuation = source.Settings.AutoPagination.RepeatHeaderOnContinuation,
                        RepeatFooterOnContinuation = source.Settings.AutoPagination.RepeatFooterOnContinuation,
                        SplitParagraphs = source.Settings.AutoPagination.SplitParagraphs,
                        SplitTables = source.Settings.AutoPagination.SplitTables
                    }
                },
            Header = CloneSection(source.Header),
            Body = CloneSection(source.Body) ?? new SectionDefinition(),
            Footer = CloneSection(source.Footer)
        };
    }

    private static SectionDefinition? CloneSection(SectionDefinition? source)
    {
        if (source == null)
            return null;

        return new SectionDefinition
        {
            Height = source.Height,
            Background = source.Background,
            Elements = source.Elements.Select(element => element.Clone()).ToList()
        };
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
            "A4" => PageSize.A4,
            "A3" => PageSize.A3,
            "A5" => PageSize.A5,
            "LETTER" => PageSize.Letter,
            "LEGAL" => PageSize.Legal,
            "TABLOID" => PageSize.Tabloid,
            _ => PageSize.A4
        };

        page.Orientation = settings.Orientation?.ToUpperInvariant() switch
        {
            "LANDSCAPE" => PageOrientation.Landscape,
            _ => PageOrientation.Portrait
        };
    }
}