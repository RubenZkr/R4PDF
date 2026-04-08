using PdfSharpCore.Drawing;
using R4PDF.Models;
using R4PDF.Models.Elements;
using R4PDF.Parsing;

namespace R4PDF.Rendering;

/// <summary>
///     Renders a single page: sets up the page, renders header/footer, and dispatches body elements
///     to the appropriate typed renderers.
/// </summary>
public class PageRenderer
{
    public sealed class AutoPaginationLayoutResult
    {
        public required PageDefinition RenderedPage { get; init; }
        public SectionDefinition? OverflowBody { get; init; }
        public bool BodyConsumedAnyContent { get; init; }
    }

    private readonly ImageRenderer _imageRenderer = new();
    private readonly LineRenderer _lineRenderer = new();
    private readonly ParagraphRenderer _paragraphRenderer = new();
    private readonly RectangleRenderer _rectangleRenderer = new();
    private readonly StyleResolver _styleResolver;
    private readonly TableRenderer _tableRenderer = new();
    private readonly TextRenderer _textRenderer = new();

    public PageRenderer(StyleResolver styleResolver)
    {
        _styleResolver = styleResolver;
    }

    public void Render(XGraphics gfx, PageDefinition page, PageSettings documentSettings, int pageNumber, int pageCount)
    {
        var settings = page.Settings ?? documentSettings;
        var margins = settings.Margins;
        var marginLeft = UnitConverter.ToPoints(margins.Left);
        var marginTop = UnitConverter.ToPoints(margins.Top);
        var marginRight = UnitConverter.ToPoints(margins.Right);
        var marginBottom = UnitConverter.ToPoints(margins.Bottom);

        var pageWidth = gfx.PageSize.Width;
        var pageHeight = gfx.PageSize.Height;
        var contentWidth = pageWidth - marginLeft - marginRight;

        var currentY = marginTop;

        if (page.Header != null)
            currentY += RenderSection(gfx, page.Header, marginLeft, currentY, contentWidth, pageNumber, pageCount);

        currentY += RenderSection(gfx, page.Body, marginLeft, currentY, contentWidth, pageNumber, pageCount);

        if (page.Footer != null)
        {
            var footerHeight = page.Footer.Height != null
                ? UnitConverter.ToPoints(page.Footer.Height)
                : PdfDefaults.DefaultFooterHeight;

            var footerY = pageHeight - marginBottom - footerHeight;
            RenderSection(gfx, page.Footer, marginLeft, footerY, contentWidth, pageNumber, pageCount);
        }
    }

    public AutoPaginationLayoutResult RenderWithAutoPagination(XGraphics gfx, PageDefinition page,
        PageSettings documentSettings, int pageNumber, int pageCount, AutoPaginationSettings autoPagination)
    {
        var settings = page.Settings ?? documentSettings;
        var margins = settings.Margins;
        var marginLeft = UnitConverter.ToPoints(margins.Left);
        var marginTop = UnitConverter.ToPoints(margins.Top);
        var marginRight = UnitConverter.ToPoints(margins.Right);
        var marginBottom = UnitConverter.ToPoints(margins.Bottom);

        var pageWidth = gfx.PageSize.Width;
        var pageHeight = gfx.PageSize.Height;
        var contentWidth = pageWidth - marginLeft - marginRight;

        var currentY = marginTop;

        if (page.Header != null)
            currentY += RenderSection(gfx, page.Header, marginLeft, currentY, contentWidth, pageNumber, pageCount);

        var footerHeight = page.Footer?.Height != null
            ? UnitConverter.ToPoints(page.Footer.Height)
            : page.Footer != null
                ? PdfDefaults.DefaultFooterHeight
                : 0;

        var bodyBottomY = pageHeight - marginBottom - footerHeight;

        var bodyResult = RenderBodyWithOverflow(gfx, page.Body, marginLeft, currentY, contentWidth, bodyBottomY,
            pageNumber, pageCount, autoPagination);

        if (page.Footer != null)
        {
            var footerY = pageHeight - marginBottom - footerHeight;
            RenderSection(gfx, page.Footer, marginLeft, footerY, contentWidth, pageNumber, pageCount);
        }

        var renderedPage = new PageDefinition
        {
            Settings = page.Settings,
            Header = CloneSection(page.Header),
            Body = new SectionDefinition
            {
                Height = page.Body.Height,
                Background = page.Body.Background,
                Elements = bodyResult.RenderedElements
            },
            Footer = CloneSection(page.Footer)
        };

        return new AutoPaginationLayoutResult
        {
            RenderedPage = renderedPage,
            OverflowBody = bodyResult.OverflowElements.Count == 0
                ? null
                : new SectionDefinition
                {
                    Height = page.Body.Height,
                    Background = page.Body.Background,
                    Elements = bodyResult.OverflowElements
                },
            BodyConsumedAnyContent = bodyResult.BodyConsumedAnyContent
        };
    }

    private double RenderSection(XGraphics gfx, SectionDefinition section, double x, double y, double contentWidth,
        int pageNumber, int pageCount)
    {
        double totalHeight = 0;

        if (section.Background != null)
        {
            var bgColor = ColorParser.Parse(section.Background);
            var sectionHeight = section.Height != null ? UnitConverter.ToPoints(section.Height) : 0;
            if (sectionHeight > 0)
                gfx.DrawRectangle(new XSolidBrush(bgColor), x, y, contentWidth, sectionHeight);
        }

        foreach (var element in section.Elements)
        {
            var elementHeight = RenderElement(gfx, element, x, y + totalHeight, contentWidth, pageNumber, pageCount);
            totalHeight += elementHeight;
        }

        return totalHeight;
    }

    private (List<PdfElement> RenderedElements, List<PdfElement> OverflowElements, bool BodyConsumedAnyContent)
        RenderBodyWithOverflow(XGraphics gfx, SectionDefinition section, double x, double y, double contentWidth,
            double bodyBottomY, int pageNumber, int pageCount, AutoPaginationSettings autoPagination)
    {
        double totalHeight = 0;
        var rendered = new List<PdfElement>();
        var overflow = new List<PdfElement>();
        var bodyConsumedAnyContent = false;

        if (section.Background != null)
        {
            var bgColor = ColorParser.Parse(section.Background);
            var sectionHeight = section.Height != null ? UnitConverter.ToPoints(section.Height) : 0;
            if (sectionHeight > 0)
                gfx.DrawRectangle(new XSolidBrush(bgColor), x, y, contentWidth, sectionHeight);
        }

        for (var i = 0; i < section.Elements.Count; i++)
        {
            var element = section.Elements[i];
            var availableHeight = bodyBottomY - (y + totalHeight);

            if (availableHeight <= 0)
            {
                overflow.Add(CloneElement(element));
                AppendRemainingOverflow(section, i + 1, overflow);
                break;
            }

            switch (element)
            {
                case ParagraphElement paragraph:
                {
                    var resolved = ResolveParagraphPlaceholders(paragraph, pageNumber, pageCount);
                    var splitResult = _paragraphRenderer.RenderWithContinuation(gfx, resolved,
                        _styleResolver.Resolve(paragraph), x, y + totalHeight, contentWidth, availableHeight,
                        autoPagination.SplitParagraphs);

                    if (splitResult.RenderedPart != null)
                    {
                        rendered.Add(CloneElement(splitResult.RenderedPart));
                        totalHeight += splitResult.HeightUsed;
                        bodyConsumedAnyContent = true;
                    }

                    if (splitResult.Remainder != null)
                    {
                        overflow.Add(CloneElement(splitResult.Remainder));
                        AppendRemainingOverflow(section, i + 1, overflow);
                        i = section.Elements.Count;
                    }

                    break;
                }
                case TableElement table:
                {
                    var splitResult = _tableRenderer.RenderWithContinuation(gfx, table, _styleResolver.Resolve(table),
                        x, y + totalHeight, contentWidth, availableHeight, autoPagination.SplitTables);

                    if (splitResult.RenderedPart != null)
                    {
                        rendered.Add(CloneElement(splitResult.RenderedPart));
                        totalHeight += splitResult.HeightUsed;
                        bodyConsumedAnyContent = true;
                    }

                    if (splitResult.Remainder != null)
                    {
                        overflow.Add(CloneElement(splitResult.Remainder));
                        AppendRemainingOverflow(section, i + 1, overflow);
                        i = section.Elements.Count;
                    }

                    break;
                }
                default:
                {
                    var elementHeight = MeasureElementHeight(gfx, element, contentWidth, pageNumber, pageCount);
                    if (elementHeight > availableHeight)
                    {
                        overflow.Add(CloneElement(element));
                        AppendRemainingOverflow(section, i + 1, overflow);
                        i = section.Elements.Count;
                        break;
                    }

                    totalHeight += RenderElement(gfx, element, x, y + totalHeight, contentWidth, pageNumber, pageCount);
                    rendered.Add(CloneElement(element));
                    bodyConsumedAnyContent = true;
                    break;
                }
            }
        }

        return (rendered, overflow, bodyConsumedAnyContent);
    }

    private static void AppendRemainingOverflow(SectionDefinition section, int startIndex, List<PdfElement> overflow)
    {
        for (var j = startIndex; j < section.Elements.Count; j++)
            overflow.Add(CloneElement(section.Elements[j]));
    }

    private double MeasureElementHeight(XGraphics gfx, PdfElement element, double contentWidth, int pageNumber,
        int pageCount)
    {
        var style = _styleResolver.Resolve(element);

        return element switch
        {
            TextElement text => MeasureTextHeight(gfx, text, style, contentWidth, pageNumber, pageCount),
            ImageElement image => _imageRenderer.MeasureHeight(image, contentWidth),
            LineElement line => MeasureLineHeight(line),
            RectangleElement rect => rect.Height != null
                ? UnitConverter.ToPoints(rect.Height)
                : PdfDefaults.DefaultRectangleHeight,
            _ => 0
        };
    }

    public double RenderElement(XGraphics gfx, PdfElement element, double x, double y, double contentWidth,
        int pageNumber, int pageCount)
    {
        var drawX = element.X != null ? UnitConverter.ToPoints(element.X) : x;
        var drawY = element.Y != null ? UnitConverter.ToPoints(element.Y) : y;
        var style = _styleResolver.Resolve(element);

        return element switch
        {
            TextElement text => RenderText(gfx, text, style, drawX, drawY, contentWidth, pageNumber, pageCount),
            ParagraphElement para => RenderParagraph(gfx, para, style, drawX, drawY, contentWidth, pageNumber,
                pageCount),
            TableElement table => _tableRenderer.Render(gfx, table, style, drawX, drawY, contentWidth),
            ImageElement image => _imageRenderer.Render(gfx, image, drawX, drawY, contentWidth),
            LineElement line => _lineRenderer.Render(gfx, line, drawX, drawY, contentWidth),
            RectangleElement rect => _rectangleRenderer.Render(gfx, rect, drawX, drawY, contentWidth),
            _ => 0
        };
    }

    private double MeasureTextHeight(XGraphics gfx, TextElement text, ResolvedStyle style, double contentWidth,
        int pageNumber, int pageCount)
    {
        var resolvedText = text.Text
            .Replace(Placeholders.PageNumber, pageNumber.ToString())
            .Replace(Placeholders.PageCount, pageCount.ToString());

        var resolved = new TextElement
        {
            Text = resolvedText,
            Style = text.Style,
            InlineStyle = text.InlineStyle,
            X = text.X,
            Y = text.Y,
            Width = text.Width,
            Height = text.Height,
            FontFamily = text.FontFamily,
            FontSize = text.FontSize,
            FontWeight = text.FontWeight,
            Color = text.Color,
            Alignment = text.Alignment
        };

        return _textRenderer.MeasureHeight(gfx, resolved, style, contentWidth);
    }

    private static double MeasureLineHeight(LineElement line)
    {
        var strokeWidth = UnitConverter.ToPoints(line.StrokeWidth, PdfDefaults.DefaultStrokeWidth);
        if (line.Y1 != null && line.Y2 != null)
        {
            var y1 = UnitConverter.ToPoints(line.Y1);
            var y2 = UnitConverter.ToPoints(line.Y2);
            return Math.Abs(y2 - y1) + strokeWidth;
        }

        return strokeWidth;
    }

    private double RenderText(XGraphics gfx, TextElement text, ResolvedStyle style, double x, double y,
        double contentWidth, int pageNumber, int pageCount)
    {
        var resolvedText = text.Text
            .Replace(Placeholders.PageNumber, pageNumber.ToString())
            .Replace(Placeholders.PageCount, pageCount.ToString());

        var original = text.Text;
        text.Text = resolvedText;
        var height = _textRenderer.Render(gfx, text, style, x, y, contentWidth);
        text.Text = original;
        return height;
    }

    private double RenderParagraph(XGraphics gfx, ParagraphElement para, ResolvedStyle style, double x, double y,
        double contentWidth, int pageNumber, int pageCount)
    {
        var resolved = ResolveParagraphPlaceholders(para, pageNumber, pageCount);
        return _paragraphRenderer.Render(gfx, resolved, style, x, y, contentWidth);
    }

    private static ParagraphElement ResolveParagraphPlaceholders(ParagraphElement paragraph, int pageNumber,
        int pageCount)
    {
        return new ParagraphElement
        {
            Style = paragraph.Style,
            InlineStyle = paragraph.InlineStyle,
            X = paragraph.X,
            Y = paragraph.Y,
            Width = paragraph.Width,
            Height = paragraph.Height,
            Content = paragraph.Content
                .Replace(Placeholders.PageNumber, pageNumber.ToString())
                .Replace(Placeholders.PageCount, pageCount.ToString()),
            FontFamily = paragraph.FontFamily,
            FontSize = paragraph.FontSize,
            FontWeight = paragraph.FontWeight,
            Color = paragraph.Color,
            Alignment = paragraph.Alignment,
            LineHeight = paragraph.LineHeight,
            Spacing = paragraph.Spacing == null
                ? null
                : new SpacingSettings
                {
                    Before = paragraph.Spacing.Before,
                    After = paragraph.Spacing.After,
                    LineHeight = paragraph.Spacing.LineHeight
                }
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

    private static PdfElement CloneElement(PdfElement source)
    {
        return source switch
        {
            TextElement text => new TextElement
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
            ParagraphElement paragraph => new ParagraphElement
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
                    : new SpacingSettings
                    {
                        Before = paragraph.Spacing.Before,
                        After = paragraph.Spacing.After,
                        LineHeight = paragraph.Spacing.LineHeight
                    }
            },
            TableElement table => new TableElement
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
                Columns = table.Columns.Select(c => new TableColumn
                {
                    Name = c.Name,
                    Width = c.Width,
                    Alignment = c.Alignment
                }).ToList(),
                Rows = table.Rows.Select(r => new TableRow
                {
                    Cells = r.Cells.ToList(),
                    BackgroundColor = r.BackgroundColor,
                    TextColor = r.TextColor
                }).ToList()
            },
            ImageElement image => new ImageElement
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
            LineElement line => new LineElement
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
            RectangleElement rect => new RectangleElement
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
            _ => throw new NotSupportedException($"Unsupported element type for cloning: {source.GetType().Name}")
        };
    }
}
