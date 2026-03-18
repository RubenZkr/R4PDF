using R4PDF.Fluent.Themes;
using R4PDF.Models;

namespace R4PDF.Tests;

public class ThemeTests
{
    // ── Default theme ────────────────────────────────────────────────────

    [Fact]
    public void DefaultTheme_HasAllStyleKeys()
    {
        var theme = PdfTheme.Default;
        var styles = theme.ToStylesDictionary();

        Assert.Contains(ThemeStyleNames.Text, styles.Keys);
        Assert.Contains(ThemeStyleNames.Heading1, styles.Keys);
        Assert.Contains(ThemeStyleNames.Heading2, styles.Keys);
        Assert.Contains(ThemeStyleNames.Heading3, styles.Keys);
        Assert.Contains(ThemeStyleNames.Accent, styles.Keys);
        Assert.Contains(ThemeStyleNames.Muted, styles.Keys);
        Assert.Contains(ThemeStyleNames.Caption, styles.Keys);
        Assert.Contains(ThemeStyleNames.Paragraph, styles.Keys);
    }

    [Fact]
    public void DefaultTheme_Heading1_IsBold()
    {
        var theme = PdfTheme.Default;
        Assert.Equal(FontWeights.Bold, theme.Heading1.FontWeight);
    }

    [Fact]
    public void DefaultTheme_Table_HasValidDefaults()
    {
        var table = PdfTheme.Default.Table;
        Assert.Equal("#003366", table.HeaderBackgroundColor);
        Assert.Equal("#FFFFFF", table.HeaderTextColor);
        Assert.True(table.AlternateRowColors);
    }

    [Fact]
    public void DefaultTheme_PageSettings_IsA4Portrait()
    {
        var settings = PdfTheme.Default.PageSettings;
        Assert.Equal(PageSizes.A4, settings.PageSize);
        Assert.Equal(Orientations.Portrait, settings.Orientation);
    }

    // ── Dark theme ───────────────────────────────────────────────────────

    [Fact]
    public void DarkTheme_HasLightTextColor()
    {
        var theme = PdfTheme.Dark;
        Assert.Equal("#E0E0E0", theme.Text.Color);
    }

    [Fact]
    public void DarkTheme_HasAllStyleKeys()
    {
        var styles = PdfTheme.Dark.ToStylesDictionary();
        Assert.Equal(8, styles.Count);
    }

    // ── Modern theme ─────────────────────────────────────────────────────

    [Fact]
    public void ModernTheme_HasLargerHeadings()
    {
        var theme = PdfTheme.Modern;
        Assert.Equal(28d, theme.Heading1.FontSize);
        Assert.Equal(20d, theme.Heading2.FontSize);
    }

    [Fact]
    public void ModernTheme_HasWiderMargins()
    {
        var settings = PdfTheme.Modern.PageSettings;
        Assert.Equal("25mm", settings.Margins.Top);
        Assert.Equal("20mm", settings.Margins.Left);
    }

    // ── Theme cloning ────────────────────────────────────────────────────

    [Fact]
    public void Clone_ProducesIndependentCopy()
    {
        var original = PdfTheme.Default;
        var clone = original.Clone();

        clone.Heading1.Color = "#FF0000";
        clone.Table.HeaderBackgroundColor = "#111111";

        Assert.NotEqual("#FF0000", original.Heading1.Color);
        Assert.NotEqual("#111111", original.Table.HeaderBackgroundColor);
    }

    // ── PdfThemeBuilder ──────────────────────────────────────────────────

    [Fact]
    public void ThemeBuilder_OverridesHeading1Color()
    {
        var theme = new PdfThemeBuilder(PdfTheme.Default)
            .Heading1(s => s.Color = "#FF0000")
            .Build();

        Assert.Equal("#FF0000", theme.Heading1.Color);
        // Other properties from base theme should remain
        Assert.Equal(FontWeights.Bold, theme.Heading1.FontWeight);
    }

    [Fact]
    public void ThemeBuilder_OverridesTableTheme()
    {
        var theme = new PdfThemeBuilder(PdfTheme.Default)
            .Table(t => t.HeaderBackgroundColor = "#222222")
            .Build();

        Assert.Equal("#222222", theme.Table.HeaderBackgroundColor);
        // Other table properties from base should remain
        Assert.Equal("#FFFFFF", theme.Table.HeaderTextColor);
    }

    [Fact]
    public void ThemeBuilder_OverridesMultipleComponents()
    {
        var theme = new PdfThemeBuilder(PdfTheme.Modern)
            .Text(s => s.FontSize = 13)
            .Heading1(s => s.Color = "#AA0000")
            .Line(l => l.Color = "#000000")
            .Footer(f => f.Height = "20mm")
            .Build();

        Assert.Equal(13d, theme.Text.FontSize);
        Assert.Equal("#AA0000", theme.Heading1.Color);
        Assert.Equal("#000000", theme.Line.Color);
        Assert.Equal("20mm", theme.Footer.Height);
    }

    [Fact]
    public void ThemeBuilder_DoesNotMutateBaseTheme()
    {
        var baseTheme = PdfTheme.Default;
        var originalColor = baseTheme.Heading1.Color;

        _ = new PdfThemeBuilder(baseTheme)
            .Heading1(s => s.Color = "#AABBCC")
            .Build();

        Assert.Equal(originalColor, baseTheme.Heading1.Color);
    }

    [Fact]
    public void ThemeBuilder_DefaultConstructor_UsesDefaultTheme()
    {
        var theme = new PdfThemeBuilder()
            .Build();

        var defaultTheme = PdfTheme.Default;
        Assert.Equal(defaultTheme.Heading1.FontSize, theme.Heading1.FontSize);
    }

    // ── ToStylesDictionary ───────────────────────────────────────────────

    [Fact]
    public void ToStylesDictionary_SkipsUnconfiguredStyles()
    {
        var theme = new PdfTheme(); // All styles are default (empty PdfStyle)
        var styles = theme.ToStylesDictionary();

        Assert.Empty(styles);
    }

    [Fact]
    public void ToStylesDictionary_IncludesConfiguredStyles()
    {
        var theme = new PdfTheme
        {
            Heading1 = new PdfStyle { FontSize = 20, FontWeight = FontWeights.Bold }
        };
        var styles = theme.ToStylesDictionary();

        Assert.Single(styles);
        Assert.Contains(ThemeStyleNames.Heading1, styles.Keys);
        Assert.Equal(20d, styles[ThemeStyleNames.Heading1].FontSize);
    }
}