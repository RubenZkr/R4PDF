using R4PDF.Models;

namespace R4PDF.Fluent.Themes;

/// <summary>
/// Factory methods for built-in themes.
/// </summary>
internal static class BuiltInThemes
{
    /// <summary>
    /// Clean professional light theme — blues, grays, Liberation Sans.
    /// </summary>
    public static PdfTheme CreateDefault() => new()
    {
        PageSettings = new PageSettings
        {
            PageSize = PageSizes.A4,
            Orientation = Orientations.Portrait,
            Margins = new MarginSettings
            {
                Top = "20mm", Bottom = "20mm", Left = "15mm", Right = "15mm"
            }
        },
        Text = new PdfStyle
        {
            FontFamily = FontFamilies.LiberationSans,
            FontSize = 11,
            Color = "#333333"
        },
        Heading1 = new PdfStyle
        {
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Color = "#003366"
        },
        Heading2 = new PdfStyle
        {
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Color = "#16213e"
        },
        Heading3 = new PdfStyle
        {
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Color = "#0f3460"
        },
        Accent = new PdfStyle
        {
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Color = "#e94560"
        },
        Muted = new PdfStyle
        {
            FontSize = 9,
            Color = "#888888"
        },
        Caption = new PdfStyle
        {
            FontSize = 9,
            Color = "#888888",
            Alignment = Alignments.Center
        },
        Paragraph = new PdfStyle
        {
            FontSize = 11,
            Color = "#333333",
            LineHeight = 1.5
        },
        Table = new TableTheme
        {
            HeaderBackgroundColor = "#003366",
            HeaderTextColor = "#FFFFFF",
            HeaderFontWeight = FontWeights.Bold,
            AlternateRowColors = true,
            AlternateColor = "#F8F9FA",
            BorderColor = "#DEE2E6",
            BorderWidth = "0.5pt"
        },
        Line = new LineTheme
        {
            Color = "#003366",
            StrokeWidth = "1pt"
        },
        Header = new SectionTheme
        {
            Height = "25mm",
            TextStyle = new PdfStyle
            {
                FontSize = 10,
                Color = "#666666"
            }
        },
        Footer = new SectionTheme
        {
            Height = "15mm",
            TextStyle = new PdfStyle
            {
                FontSize = 9,
                Color = "#888888",
                Alignment = Alignments.Center
            }
        }
    };

    /// <summary>
    /// Dark background theme with light text and vibrant accents.
    /// </summary>
    public static PdfTheme CreateDark() => new()
    {
        PageSettings = new PageSettings
        {
            PageSize = PageSizes.A4,
            Orientation = Orientations.Portrait,
            Margins = new MarginSettings
            {
                Top = "20mm", Bottom = "20mm", Left = "15mm", Right = "15mm"
            }
        },
        Text = new PdfStyle
        {
            FontFamily = FontFamilies.LiberationSans,
            FontSize = 11,
            Color = "#E0E0E0"
        },
        Heading1 = new PdfStyle
        {
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Color = "#64B5F6"
        },
        Heading2 = new PdfStyle
        {
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Color = "#90CAF9"
        },
        Heading3 = new PdfStyle
        {
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Color = "#BBDEFB"
        },
        Accent = new PdfStyle
        {
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Color = "#FF7043"
        },
        Muted = new PdfStyle
        {
            FontSize = 9,
            Color = "#9E9E9E"
        },
        Caption = new PdfStyle
        {
            FontSize = 9,
            Color = "#9E9E9E",
            Alignment = Alignments.Center
        },
        Paragraph = new PdfStyle
        {
            FontSize = 11,
            Color = "#E0E0E0",
            LineHeight = 1.5
        },
        Table = new TableTheme
        {
            HeaderBackgroundColor = "#1565C0",
            HeaderTextColor = "#FFFFFF",
            HeaderFontWeight = FontWeights.Bold,
            AlternateRowColors = true,
            AlternateColor = "#263238",
            BorderColor = "#455A64",
            BorderWidth = "0.5pt"
        },
        Line = new LineTheme
        {
            Color = "#455A64",
            StrokeWidth = "1pt"
        },
        Header = new SectionTheme
        {
            Height = "25mm",
            TextStyle = new PdfStyle
            {
                FontSize = 10,
                Color = "#B0BEC5"
            }
        },
        Footer = new SectionTheme
        {
            Height = "15mm",
            TextStyle = new PdfStyle
            {
                FontSize = 9,
                Color = "#78909C",
                Alignment = Alignments.Center
            }
        }
    };

    /// <summary>
    /// Minimalist modern theme with generous spacing and clean typography.
    /// </summary>
    public static PdfTheme CreateModern() => new()
    {
        PageSettings = new PageSettings
        {
            PageSize = PageSizes.A4,
            Orientation = Orientations.Portrait,
            Margins = new MarginSettings
            {
                Top = "25mm", Bottom = "25mm", Left = "20mm", Right = "20mm"
            }
        },
        Text = new PdfStyle
        {
            FontFamily = FontFamilies.LiberationSans,
            FontSize = 11,
            Color = "#2C3E50"
        },
        Heading1 = new PdfStyle
        {
            FontSize = 28,
            FontWeight = FontWeights.Bold,
            Color = "#2C3E50"
        },
        Heading2 = new PdfStyle
        {
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Color = "#34495E"
        },
        Heading3 = new PdfStyle
        {
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            Color = "#7F8C8D"
        },
        Accent = new PdfStyle
        {
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Color = "#E74C3C"
        },
        Muted = new PdfStyle
        {
            FontSize = 9,
            Color = "#BDC3C7"
        },
        Caption = new PdfStyle
        {
            FontSize = 9,
            Color = "#95A5A6",
            Alignment = Alignments.Center
        },
        Paragraph = new PdfStyle
        {
            FontSize = 11,
            Color = "#2C3E50",
            LineHeight = 1.7
        },
        Table = new TableTheme
        {
            HeaderBackgroundColor = "#2C3E50",
            HeaderTextColor = "#FFFFFF",
            HeaderFontWeight = FontWeights.Bold,
            AlternateRowColors = true,
            AlternateColor = "#F9FAFB",
            BorderColor = "#E5E7EB",
            BorderWidth = "0.5pt"
        },
        Line = new LineTheme
        {
            Color = "#E5E7EB",
            StrokeWidth = "0.5pt"
        },
        Header = new SectionTheme
        {
            Height = "20mm",
            TextStyle = new PdfStyle
            {
                FontSize = 9,
                Color = "#95A5A6"
            }
        },
        Footer = new SectionTheme
        {
            Height = "15mm",
            TextStyle = new PdfStyle
            {
                FontSize = 8,
                Color = "#BDC3C7",
                Alignment = Alignments.Center
            }
        }
    };
}
