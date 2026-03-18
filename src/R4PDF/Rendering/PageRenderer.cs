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

        // Render header
        if (page.Header != null)
            currentY += RenderSection(gfx, page.Header, marginLeft, currentY, contentWidth, pageNumber, pageCount);

        // Render body elements
        currentY += RenderSection(gfx, page.Body, marginLeft, currentY, contentWidth, pageNumber, pageCount);

        // Render footer at the bottom of the page
        if (page.Footer != null)
        {
            var footerHeight = page.Footer.Height != null
                ? UnitConverter.ToPoints(page.Footer.Height)
                : PdfDefaults.DefaultFooterHeight;

            var footerY = pageHeight - marginBottom - footerHeight;
            RenderSection(gfx, page.Footer, marginLeft, footerY, contentWidth, pageNumber, pageCount);
        }
    }

    private double RenderSection(XGraphics gfx, SectionDefinition section, double x, double y, double contentWidth,
        int pageNumber, int pageCount)
    {
        double totalHeight = 0;

        // Draw section background if specified
        if (section.Background != null)
        {
            var bgColor = ColorParser.Parse(section.Background);
            var sectionHeight = section.Height != null ? UnitConverter.ToPoints(section.Height) : 0;
            if (sectionHeight > 0) gfx.DrawRectangle(new XSolidBrush(bgColor), x, y, contentWidth, sectionHeight);
        }

        foreach (var element in section.Elements)
        {
            var elementHeight = RenderElement(gfx, element, x, y + totalHeight, contentWidth, pageNumber, pageCount);
            totalHeight += elementHeight;
        }

        return totalHeight;
    }

    public double RenderElement(XGraphics gfx, PdfElement element, double x, double y, double contentWidth,
        int pageNumber, int pageCount)
    {
        // Resolve position overrides
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

    private double RenderText(XGraphics gfx, TextElement text, ResolvedStyle style, double x, double y,
        double contentWidth, int pageNumber, int pageCount)
    {
        // Replace page number placeholders
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
        var resolvedContent = para.Content
            .Replace(Placeholders.PageNumber, pageNumber.ToString())
            .Replace(Placeholders.PageCount, pageCount.ToString());

        var original = para.Content;
        para.Content = resolvedContent;
        var height = _paragraphRenderer.Render(gfx, para, style, x, y, contentWidth);
        para.Content = original;
        return height;
    }
}