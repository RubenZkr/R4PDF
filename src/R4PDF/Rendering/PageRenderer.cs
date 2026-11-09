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
    private LayoutRecorder? _recorder;

    public PageRenderer(StyleResolver styleResolver)
    {
        _styleResolver = styleResolver;
    }

    /// <summary>Records anchors and links; set for the final render pass only, not for the layout passes.</summary>
    internal LayoutRecorder? Recorder
    {
        get => _recorder;
        set
        {
            _recorder = value;
            _tableRenderer.Recorder = value;
        }
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
                overflow.Add(element.Clone());
                AppendRemainingOverflow(section, i + 1, overflow);
                break;
            }

            if (element.KeepWithNext && bodyConsumedAnyContent
                && KeepWithNextHeight(gfx, section.Elements, i, contentWidth, pageNumber, pageCount) > availableHeight)
            {
                overflow.Add(element.Clone());
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
                        rendered.Add(splitResult.RenderedPart.Clone());
                        totalHeight += splitResult.HeightUsed;
                        bodyConsumedAnyContent = true;
                    }

                    if (splitResult.Remainder != null)
                    {
                        overflow.Add(splitResult.Remainder.Clone());
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
                        rendered.Add(splitResult.RenderedPart.Clone());
                        totalHeight += splitResult.HeightUsed;
                        bodyConsumedAnyContent = true;
                    }

                    if (splitResult.Remainder != null)
                    {
                        overflow.Add(splitResult.Remainder.Clone());
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
                        overflow.Add(element.Clone());
                        AppendRemainingOverflow(section, i + 1, overflow);
                        i = section.Elements.Count;
                        break;
                    }

                    totalHeight += RenderElement(gfx, element, x, y + totalHeight, contentWidth, pageNumber, pageCount);
                    rendered.Add(element.Clone());
                    bodyConsumedAnyContent = true;
                    break;
                }
            }
        }

        return (rendered, overflow, bodyConsumedAnyContent);
    }

    /// <summary>
    ///     The height needed to keep the chain of elements from <paramref name="index" /> on (all with
    ///     <see cref="PdfElement.KeepWithNext" />) together with the start of the element after it.
    /// </summary>
    private double KeepWithNextHeight(XGraphics gfx, IReadOnlyList<PdfElement> elements, int index, double contentWidth,
        int pageNumber, int pageCount)
    {
        var height = 0d;
        var next = index;
        for (; next < elements.Count && elements[next].KeepWithNext; next++)
            height += MeasureHeights(gfx, elements[next], contentWidth, pageNumber, pageCount).Full;

        if (next < elements.Count)
            height += MeasureHeights(gfx, elements[next], contentWidth, pageNumber, pageCount).Start;

        return height;
    }

    private (double Full, double Start) MeasureHeights(XGraphics gfx, PdfElement element, double contentWidth,
        int pageNumber, int pageCount)
    {
        switch (element)
        {
            case ParagraphElement paragraph:
                return _paragraphRenderer.Measure(gfx, ResolveParagraphPlaceholders(paragraph, pageNumber, pageCount),
                    _styleResolver.Resolve(paragraph), contentWidth);
            case TableElement table:
                return _tableRenderer.Measure(gfx, table, _styleResolver.Resolve(table), contentWidth);
            default:
                var height = MeasureElementHeight(gfx, element, contentWidth, pageNumber, pageCount);
                return (height, height);
        }
    }

    private static void AppendRemainingOverflow(SectionDefinition section, int startIndex, List<PdfElement> overflow)
    {
        for (var j = startIndex; j < section.Elements.Count; j++)
            overflow.Add(section.Elements[j].Clone());
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
            CanvasElement canvas => MeasureCanvasHeight(gfx, canvas, contentWidth, pageNumber, pageCount),
            _ => 0
        };
    }

    public double RenderElement(XGraphics gfx, PdfElement element, double x, double y, double contentWidth,
        int pageNumber, int pageCount)
    {
        var drawX = element.X != null ? UnitConverter.ToPoints(element.X) : x;
        var drawY = element.Y != null ? UnitConverter.ToPoints(element.Y) : y;
        return RenderElementAt(gfx, element, drawX, drawY, contentWidth, pageNumber, pageCount);
    }

    /// <summary>
    ///     Renders an element at a resolved position and records its anchor and link. Lines with absolute
    ///     coordinates are moved by <paramref name="originX" /> and <paramref name="originY" />, the corner
    ///     of the canvas they are in.
    /// </summary>
    private double RenderElementAt(XGraphics gfx, PdfElement element, double x, double y, double contentWidth,
        int pageNumber, int pageCount, double originX = 0, double originY = 0)
    {
        var style = _styleResolver.Resolve(element);

        var height = element switch
        {
            TextElement text => RenderText(gfx, text, style, x, y, contentWidth, pageNumber, pageCount),
            ParagraphElement para => RenderParagraph(gfx, para, style, x, y, contentWidth, pageNumber,
                pageCount),
            TableElement table => _tableRenderer.Render(gfx, table, style, x, y, contentWidth),
            ImageElement image => _imageRenderer.Render(gfx, image, x, y, contentWidth),
            LineElement line => _lineRenderer.Render(gfx, line, x, y, contentWidth, originX, originY),
            RectangleElement rect => _rectangleRenderer.Render(gfx, rect, x, y, contentWidth),
            CanvasElement canvas => RenderCanvas(gfx, canvas, x, y, contentWidth, pageNumber, pageCount),
            _ => 0
        };

        var width = element.Width != null ? UnitConverter.ToPoints(element.Width) : contentWidth;
        _recorder?.Record(element.Anchor, element.Link, new XRect(x, y, width, height));
        return height;
    }

    /// <summary>
    ///     The elements of a canvas, each at its position relative to the corner of the canvas. Only the
    ///     canvas itself takes up room in the flow of the page.
    /// </summary>
    private double RenderCanvas(XGraphics gfx, CanvasElement canvas, double x, double y, double contentWidth,
        int pageNumber, int pageCount)
    {
        var width = canvas.Width != null ? UnitConverter.ToPoints(canvas.Width) : contentWidth;

        foreach (var child in canvas.Elements)
        {
            var left = child.X != null ? UnitConverter.ToPoints(child.X) : 0;
            var top = child.Y != null ? UnitConverter.ToPoints(child.Y) : 0;
            RenderElementAt(gfx, child, x + left, y + top, Math.Max(0, width - left), pageNumber, pageCount, x, y);
        }

        return MeasureCanvasHeight(gfx, canvas, contentWidth, pageNumber, pageCount);
    }

    /// <summary>The height of the canvas, or without one the bottom of its lowest element.</summary>
    private double MeasureCanvasHeight(XGraphics gfx, CanvasElement canvas, double contentWidth, int pageNumber,
        int pageCount)
    {
        if (canvas.Height != null)
            return UnitConverter.ToPoints(canvas.Height);

        var width = canvas.Width != null ? UnitConverter.ToPoints(canvas.Width) : contentWidth;
        var bottom = 0d;

        foreach (var child in canvas.Elements)
        {
            var left = child.X != null ? UnitConverter.ToPoints(child.X) : 0;
            var top = child.Y != null ? UnitConverter.ToPoints(child.Y) : 0;
            var childBottom = child is LineElement { Y1: not null, Y2: not null } line
                ? Math.Max(UnitConverter.ToPoints(line.Y1), UnitConverter.ToPoints(line.Y2))
                  + UnitConverter.ToPoints(line.StrokeWidth, PdfDefaults.DefaultStrokeWidth)
                : top + MeasureHeights(gfx, child, Math.Max(0, width - left), pageNumber, pageCount).Full;
            bottom = Math.Max(bottom, childBottom);
        }

        return bottom;
    }

    private double MeasureTextHeight(XGraphics gfx, TextElement text, ResolvedStyle style, double contentWidth,
        int pageNumber, int pageCount)
    {
        var resolvedText = text.Text
            .Replace(Placeholders.PageNumber, pageNumber.ToString())
            .Replace(Placeholders.PageCount, pageCount.ToString());

        var resolved = (TextElement)text.Clone();
        resolved.Text = resolvedText;

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
        return paragraph.CloneWithContent(paragraph.Content
            .Replace(Placeholders.PageNumber, pageNumber.ToString())
            .Replace(Placeholders.PageCount, pageCount.ToString()));
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
}
