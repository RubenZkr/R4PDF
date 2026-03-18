using R4PDF.Models;
using R4PDF.Models.Elements;

namespace R4PDF.Rendering;

/// <summary>
///     Merges named styles from the template's styles dictionary with inline element styles.
/// </summary>
public class StyleResolver
{
    private readonly Dictionary<string, PdfStyle> _styles;

    public StyleResolver(Dictionary<string, PdfStyle> styles)
    {
        _styles = styles ?? new Dictionary<string, PdfStyle>();
    }

    public ResolvedStyle Resolve(PdfElement element)
    {
        var resolved = new ResolvedStyle();

        // Apply named style first (base layer)
        if (!string.IsNullOrEmpty(element.Style) && _styles.TryGetValue(element.Style, out var namedStyle))
            ApplyStyle(resolved, namedStyle);

        // Apply inline style (overrides named)
        if (element.InlineStyle != null) ApplyStyle(resolved, element.InlineStyle);

        // Apply direct element properties (highest priority) for text/paragraph elements
        ApplyElementProperties(resolved, element);

        return resolved;
    }

    private static void ApplyStyle(ResolvedStyle resolved, PdfStyle style)
    {
        if (style.FontFamily != null) resolved.FontFamily = style.FontFamily;
        if (style.FontSize.HasValue) resolved.FontSize = style.FontSize.Value;
        if (style.FontWeight != null) resolved.FontWeight = style.FontWeight;
        if (style.FontStyle != null) resolved.FontStyle = style.FontStyle;
        if (style.Color != null) resolved.Color = style.Color;
        if (style.BackgroundColor != null) resolved.BackgroundColor = style.BackgroundColor;
        if (style.Alignment != null) resolved.Alignment = style.Alignment;
        if (style.LineHeight.HasValue) resolved.LineHeight = style.LineHeight.Value;
    }

    private static void ApplyElementProperties(ResolvedStyle resolved, PdfElement element)
    {
        switch (element)
        {
            case TextElement text:
                if (text.FontFamily != null) resolved.FontFamily = text.FontFamily;
                if (text.FontSize.HasValue) resolved.FontSize = text.FontSize.Value;
                if (text.FontWeight != null) resolved.FontWeight = text.FontWeight;
                if (text.Color != null) resolved.Color = text.Color;
                if (text.Alignment != null) resolved.Alignment = text.Alignment;
                break;
            case ParagraphElement para:
                if (para.FontFamily != null) resolved.FontFamily = para.FontFamily;
                if (para.FontSize.HasValue) resolved.FontSize = para.FontSize.Value;
                if (para.FontWeight != null) resolved.FontWeight = para.FontWeight;
                if (para.Color != null) resolved.Color = para.Color;
                if (para.Alignment != null) resolved.Alignment = para.Alignment;
                if (para.LineHeight.HasValue) resolved.LineHeight = para.LineHeight.Value;
                break;
        }
    }
}