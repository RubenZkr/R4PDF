using PdfSharpCore.Drawing;

namespace R4PDF.Rendering;

public class ResolvedStyle
{
    public string FontFamily { get; set; } = PdfDefaults.FontFamily;
    public double FontSize { get; set; } = PdfDefaults.FontSize;
    public string FontWeight { get; set; } = PdfDefaults.FontWeight;
    public string FontStyle { get; set; } = PdfDefaults.FontStyle;
    public string? Color { get; set; }
    public string? BackgroundColor { get; set; }
    public string Alignment { get; set; } = PdfDefaults.Alignment;
    public double LineHeight { get; set; } = PdfDefaults.LineHeight;

    public XFont ToXFont()
    {
        var style = XFontStyle.Regular;
        if (FontWeight?.Equals(FontWeights.Bold, StringComparison.OrdinalIgnoreCase) == true)
            style |= XFontStyle.Bold;
        if (FontStyle?.Equals(FontStyles.Italic, StringComparison.OrdinalIgnoreCase) == true)
            style |= XFontStyle.Italic;

        return new XFont(FontFamily, FontSize, style);
    }

    public XStringFormat ToXStringFormat()
    {
        var format = new XStringFormat();

        format.Alignment = Alignment?.ToLowerInvariant() switch
        {
            Alignments.Center => XStringAlignment.Center,
            Alignments.Right => XStringAlignment.Far,
            _ => XStringAlignment.Near
        };

        format.LineAlignment = XLineAlignment.Near;
        return format;
    }
}