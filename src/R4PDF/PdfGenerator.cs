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

    private void RenderTemplate(PdfTemplate template, Stream outputStream)
    {
        using var document = CreatePdfDocument(template);
        var styleResolver = new StyleResolver(template.Styles);
        var pageRenderer = new PageRenderer(styleResolver);

        var autoPagination = template.Settings.AutoPagination;
        var pages = autoPagination.Enabled
            ? MaterializeAutoPaginatedPages(template, pageRenderer)
            : template.Pages;

        var pageCount = pages.Count;

        for (var i = 0; i < pages.Count; i++)
        {
            var pageDefinition = pages[i];
            var settings = pageDefinition.Settings ?? template.Settings;

            var pdfPage = document.AddPage();
            ConfigurePage(pdfPage, settings);

            using var gfx = XGraphics.FromPdfPage(pdfPage);
            pageRenderer.Render(gfx, pageDefinition, template.Settings, i + 1, pageCount);
        }

        document.Save(outputStream);
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
            Elements = source.Elements.Select(CloneElement).ToList()
        };
    }

    private static Models.Elements.PdfElement CloneElement(Models.Elements.PdfElement source)
    {
        return source switch
        {
            Models.Elements.TextElement text => new Models.Elements.TextElement
            {
                Style = text.Style,
                InlineStyle = text.InlineStyle,
                X = text.X,
                Y = text.Y,
                Width = text.Width,
                Height = text.Height,
                Text = text.Text,
                FontFamily = text.FontFamily,
                FontSize = text.FontSize,
                FontWeight = text.FontWeight,
                Color = text.Color,
                Alignment = text.Alignment
            },
            Models.Elements.ParagraphElement paragraph => new Models.Elements.ParagraphElement
            {
                Style = paragraph.Style,
                InlineStyle = paragraph.InlineStyle,
                X = paragraph.X,
                Y = paragraph.Y,
                Width = paragraph.Width,
                Height = paragraph.Height,
                Content = paragraph.Content,
                FontFamily = paragraph.FontFamily,
                FontSize = paragraph.FontSize,
                FontWeight = paragraph.FontWeight,
                Color = paragraph.Color,
                Alignment = paragraph.Alignment,
                LineHeight = paragraph.LineHeight,
                Spacing = paragraph.Spacing == null
                    ? null
                    : new Models.Elements.SpacingSettings
                    {
                        Before = paragraph.Spacing.Before,
                        After = paragraph.Spacing.After,
                        LineHeight = paragraph.Spacing.LineHeight
                    }
            },
            Models.Elements.TableElement table => new Models.Elements.TableElement
            {
                Style = table.Style,
                InlineStyle = table.InlineStyle,
                X = table.X,
                Y = table.Y,
                Width = table.Width,
                Height = table.Height,
                ShowHeader = table.ShowHeader,
                AlternateRowColors = table.AlternateRowColors,
                AlternateColor = table.AlternateColor,
                HeaderStyle = table.HeaderStyle == null
                    ? null
                    : new PdfStyle
                    {
                        FontFamily = table.HeaderStyle.FontFamily,
                        FontSize = table.HeaderStyle.FontSize,
                        FontWeight = table.HeaderStyle.FontWeight,
                        FontStyle = table.HeaderStyle.FontStyle,
                        Color = table.HeaderStyle.Color,
                        BackgroundColor = table.HeaderStyle.BackgroundColor,
                        Alignment = table.HeaderStyle.Alignment,
                        LineHeight = table.HeaderStyle.LineHeight
                    },
                Borders = table.Borders == null
                    ? null
                    : new BorderStyle
                    {
                        Type = table.Borders.Type,
                        Color = table.Borders.Color,
                        Width = table.Borders.Width
                    },
                Columns = table.Columns.Select(c => new Models.Elements.TableColumn
                {
                    Name = c.Name,
                    Width = c.Width,
                    Alignment = c.Alignment
                }).ToList(),
                Rows = table.Rows.Select(r => new Models.Elements.TableRow
                {
                    Cells = r.Cells.ToList(),
                    BackgroundColor = r.BackgroundColor,
                    TextColor = r.TextColor
                }).ToList()
            },
            Models.Elements.ImageElement image => new Models.Elements.ImageElement
            {
                Style = image.Style,
                InlineStyle = image.InlineStyle,
                X = image.X,
                Y = image.Y,
                Width = image.Width,
                Height = image.Height,
                Source = image.Source,
                Alignment = image.Alignment,
                MaintainAspectRatio = image.MaintainAspectRatio
            },
            Models.Elements.LineElement line => new Models.Elements.LineElement
            {
                Style = line.Style,
                InlineStyle = line.InlineStyle,
                X = line.X,
                Y = line.Y,
                Width = line.Width,
                Height = line.Height,
                X1 = line.X1,
                Y1 = line.Y1,
                X2 = line.X2,
                Y2 = line.Y2,
                Color = line.Color,
                StrokeWidth = line.StrokeWidth,
                DashPattern = line.DashPattern
            },
            Models.Elements.RectangleElement rect => new Models.Elements.RectangleElement
            {
                Style = rect.Style,
                InlineStyle = rect.InlineStyle,
                X = rect.X,
                Y = rect.Y,
                Width = rect.Width,
                Height = rect.Height,
                FillColor = rect.FillColor,
                StrokeColor = rect.StrokeColor,
                StrokeWidth = rect.StrokeWidth,
                CornerRadius = rect.CornerRadius
            },
            _ => throw new NotSupportedException($"Unsupported element type: {source.GetType().Name}")
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