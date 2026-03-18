using R4PDF.Fluent;
using R4PDF.Fluent.Themes;
using R4PDF.Models;
using R4PDF.Models.Elements;

namespace R4PDF.Tests;

public class FluentBuilderTests
{
    // ── Pdf.Create() ─────────────────────────────────────────────────────

    [Fact]
    public void Create_ReturnsBuilder()
    {
        var builder = Pdf.Create();
        Assert.NotNull(builder);
    }

    // ── Metadata ─────────────────────────────────────────────────────────

    [Fact]
    public void WithMetadata_SetsMetadata()
    {
        var template = Pdf.Create()
            .WithMetadata(m => m.Title("Test").Author("Tester").Subject("Sub").Keywords("kw"))
            .AddPage(p => p.Body(b => b.Text("x")))
            .Build();

        Assert.Equal("Test", template.Metadata?.Title);
        Assert.Equal("Tester", template.Metadata?.Author);
        Assert.Equal("Sub", template.Metadata?.Subject);
        Assert.Equal("kw", template.Metadata?.Keywords);
    }

    // ── Settings ─────────────────────────────────────────────────────────

    [Fact]
    public void WithSettings_OverridesDefaults()
    {
        var template = Pdf.Create()
            .WithSettings(s => s.PageSize(PageSizes.Letter).Orientation(Orientations.Landscape))
            .AddPage(p => p.Body(b => b.Text("x")))
            .Build();

        Assert.Equal(PageSizes.Letter, template.Settings.PageSize);
        Assert.Equal(Orientations.Landscape, template.Settings.Orientation);
    }

    [Fact]
    public void WithSettings_UniformMargins()
    {
        var template = Pdf.Create()
            .WithSettings(s => s.Margins("30mm"))
            .AddPage(p => p.Body(b => b.Text("x")))
            .Build();

        Assert.Equal("30mm", template.Settings.Margins.Top);
        Assert.Equal("30mm", template.Settings.Margins.Left);
    }

    // ── Theme integration ────────────────────────────────────────────────

    [Fact]
    public void WithTheme_AppliesStylesToTemplate()
    {
        var template = Pdf.Create()
            .WithTheme(PdfTheme.Default)
            .AddPage(p => p.Body(b => b.Text("x")))
            .Build();

        Assert.Contains(ThemeStyleNames.Heading1, template.Styles.Keys);
        Assert.Contains(ThemeStyleNames.Text, template.Styles.Keys);
    }

    [Fact]
    public void WithTheme_SetsPageSettings()
    {
        var template = Pdf.Create()
            .WithTheme(PdfTheme.Modern)
            .AddPage(p => p.Body(b => b.Text("x")))
            .Build();

        Assert.Equal("25mm", template.Settings.Margins.Top);
    }

    [Fact]
    public void WithSettings_OverridesThemeSettings()
    {
        var template = Pdf.Create()
            .WithTheme(PdfTheme.Default)
            .WithSettings(s => s.PageSize(PageSizes.Letter))
            .AddPage(p => p.Body(b => b.Text("x")))
            .Build();

        Assert.Equal(PageSizes.Letter, template.Settings.PageSize);
    }

    // ── Custom styles ────────────────────────────────────────────────────

    [Fact]
    public void AddStyle_MergedWithThemeStyles()
    {
        var template = Pdf.Create()
            .WithTheme(PdfTheme.Default)
            .AddStyle("custom", new PdfStyle { FontSize = 42, Color = "#FF0000" })
            .AddPage(p => p.Body(b => b.Text("x")))
            .Build();

        Assert.Contains("custom", template.Styles.Keys);
        Assert.Contains(ThemeStyleNames.Heading1, template.Styles.Keys);
    }

    [Fact]
    public void AddStyle_OverridesThemeStyleWithSameKey()
    {
        var template = Pdf.Create()
            .WithTheme(PdfTheme.Default)
            .AddStyle(ThemeStyleNames.Heading1, new PdfStyle { FontSize = 99 })
            .AddPage(p => p.Body(b => b.Text("x")))
            .Build();

        Assert.Equal(99d, template.Styles[ThemeStyleNames.Heading1].FontSize);
    }

    // ── Pages ────────────────────────────────────────────────────────────

    [Fact]
    public void AddPage_CreatesPageDefinition()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.Text("Hello")))
            .Build();

        Assert.Single(template.Pages);
        Assert.Single(template.Pages[0].Body.Elements);
    }

    [Fact]
    public void AddPage_MultiplePages()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.Text("Page 1")))
            .AddPage(p => p.Body(b => b.Text("Page 2")))
            .AddPage(p => p.Body(b => b.Text("Page 3")))
            .Build();

        Assert.Equal(3, template.Pages.Count);
    }

    // ── Page sections ────────────────────────────────────────────────────

    [Fact]
    public void PageBuilder_HeaderBodyFooter()
    {
        var template = Pdf.Create()
            .AddPage(p => p
                .Header(h => h.Text("Header"))
                .Body(b => b.Text("Body"))
                .Footer(f => f.Text("Footer")))
            .Build();

        var page = template.Pages[0];
        Assert.NotNull(page.Header);
        Assert.Single(page.Header!.Elements);
        Assert.Single(page.Body.Elements);
        Assert.NotNull(page.Footer);
        Assert.Single(page.Footer!.Elements);
    }

    [Fact]
    public void PageBuilder_PerPageSettings()
    {
        var template = Pdf.Create()
            .AddPage(p => p
                .Settings(s => s.Orientation(Orientations.Landscape))
                .Body(b => b.Text("wide")))
            .Build();

        Assert.Equal(Orientations.Landscape, template.Pages[0].Settings?.Orientation);
    }

    // ── Section elements ── Text ──────────────────────────────────────────

    [Fact]
    public void SectionBuilder_Text_SetsStyleName()
    {
        var template = Pdf.Create()
            .WithTheme(PdfTheme.Default)
            .AddPage(p => p.Body(b => b.Text("Hello")))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as TextElement;
        Assert.NotNull(element);
        Assert.Equal("Hello", element!.Text);
        Assert.Equal(ThemeStyleNames.Text, element.Style);
    }

    [Fact]
    public void SectionBuilder_Text_WithOverrides()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.Text("Big", o =>
            {
                o.FontSize = 36;
                o.Color = "#FF0000";
                o.FontWeight = FontWeights.Bold;
            })))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as TextElement;
        Assert.Equal(36d, element!.FontSize);
        Assert.Equal("#FF0000", element.Color);
        Assert.Equal(FontWeights.Bold, element.FontWeight);
    }

    // ── Section elements ── Headings ──────────────────────────────────────

    [Fact]
    public void SectionBuilder_Heading1_UsesThemeStyle()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.Heading1("Title")))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as TextElement;
        Assert.Equal("Title", element!.Text);
        Assert.Equal(ThemeStyleNames.Heading1, element.Style);
    }

    [Fact]
    public void SectionBuilder_Heading2_UsesThemeStyle()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.Heading2("Subtitle")))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as TextElement;
        Assert.Equal(ThemeStyleNames.Heading2, element!.Style);
    }

    [Fact]
    public void SectionBuilder_Heading3_UsesThemeStyle()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.Heading3("Section")))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as TextElement;
        Assert.Equal(ThemeStyleNames.Heading3, element!.Style);
    }

    // ── Section elements ── Accent / Muted / Caption ─────────────────────

    [Fact]
    public void SectionBuilder_AccentText()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.AccentText("Important!")))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as TextElement;
        Assert.Equal(ThemeStyleNames.Accent, element!.Style);
    }

    [Fact]
    public void SectionBuilder_MutedText()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.MutedText("Fine print")))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as TextElement;
        Assert.Equal(ThemeStyleNames.Muted, element!.Style);
    }

    [Fact]
    public void SectionBuilder_CaptionText()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.CaptionText("Fig 1")))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as TextElement;
        Assert.Equal(ThemeStyleNames.Caption, element!.Style);
    }

    // ── Section elements ── Paragraph ─────────────────────────────────────

    [Fact]
    public void SectionBuilder_Paragraph_SetsContent()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.Paragraph("Long text here...")))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as ParagraphElement;
        Assert.NotNull(element);
        Assert.Equal("Long text here...", element!.Content);
        Assert.Equal(ThemeStyleNames.Paragraph, element.Style);
    }

    [Fact]
    public void SectionBuilder_Paragraph_WithOverrides()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.Paragraph("Text", o =>
            {
                o.FontSize = 14;
                o.LineHeight = 2.0;
            })))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as ParagraphElement;
        Assert.Equal(14d, element!.FontSize);
        Assert.Equal(2.0, element.LineHeight);
    }

    // ── Section elements ── Table ─────────────────────────────────────────

    [Fact]
    public void SectionBuilder_Table_BuildsColumnsAndRows()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.Table(t => t
                .Column("Name", "50%")
                .Column("Value", "50%", Alignments.Right)
                .Row("A", "1")
                .Row("B", "2"))))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as TableElement;
        Assert.NotNull(element);
        Assert.Equal(2, element!.Columns.Count);
        Assert.Equal("Name", element.Columns[0].Name);
        Assert.Equal("50%", element.Columns[0].Width);
        Assert.Equal(Alignments.Right, element.Columns[1].Alignment);
        Assert.Equal(2, element.Rows.Count);
        Assert.Equal("A", element.Rows[0].Cells[0]);
    }

    [Fact]
    public void SectionBuilder_Table_WithTheme_AppliesDefaults()
    {
        var template = Pdf.Create()
            .WithTheme(PdfTheme.Default)
            .AddPage(p => p.Body(b => b.Table(t => t
                .Column("Col1")
                .Row("Data"))))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as TableElement;
        Assert.Equal("#003366", element!.HeaderStyle?.BackgroundColor);
        Assert.Equal("#FFFFFF", element.HeaderStyle?.Color);
        Assert.True(element.AlternateRowColors);
    }

    [Fact]
    public void SectionBuilder_Table_UserOverridesTheme()
    {
        var template = Pdf.Create()
            .WithTheme(PdfTheme.Default)
            .AddPage(p => p.Body(b => b.Table(t => t
                .Column("Col1")
                .Row("Data")
                .HeaderStyle(s => s.BackgroundColor = "#FF0000")
                .AlternateRowColor("#EEEEEE"))))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as TableElement;
        Assert.Equal("#FF0000", element!.HeaderStyle?.BackgroundColor);
        Assert.Equal("#EEEEEE", element.AlternateColor);
    }

    // ── Section elements ── Line ──────────────────────────────────────────

    [Fact]
    public void SectionBuilder_Line_DefaultFromTheme()
    {
        var template = Pdf.Create()
            .WithTheme(PdfTheme.Default)
            .AddPage(p => p.Body(b => b.Line()))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as LineElement;
        Assert.NotNull(element);
        Assert.Equal("#003366", element!.Color);
        Assert.Equal("1pt", element.StrokeWidth);
    }

    [Fact]
    public void SectionBuilder_Line_WithOverrides()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.Line(o =>
            {
                o.Color = "#FF0000";
                o.StrokeWidth = "2pt";
                o.DashPattern = "4,2";
            })))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as LineElement;
        Assert.Equal("#FF0000", element!.Color);
        Assert.Equal("2pt", element.StrokeWidth);
        Assert.Equal("4,2", element.DashPattern);
    }

    // ── Section elements ── Rectangle ─────────────────────────────────────

    [Fact]
    public void SectionBuilder_Rectangle()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.Rectangle(o =>
            {
                o.FillColor = "#F0F0F0";
                o.Width = "170mm";
                o.Height = "30mm";
                o.CornerRadius = "5pt";
            })))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as RectangleElement;
        Assert.Equal("#F0F0F0", element!.FillColor);
        Assert.Equal("170mm", element.Width);
        Assert.Equal("30mm", element.Height);
        Assert.Equal("5pt", element.CornerRadius);
    }

    // ── Section elements ── Image ─────────────────────────────────────────

    [Fact]
    public void SectionBuilder_Image()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.Image("logo.png")))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as ImageElement;
        Assert.Equal("logo.png", element!.Source);
        Assert.True(element.MaintainAspectRatio);
    }

    [Fact]
    public void SectionBuilder_Image_WithOptions()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.Image("photo.jpg", o =>
            {
                o.Width = "100mm";
                o.Alignment = Alignments.Center;
            })))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as ImageElement;
        Assert.Equal("100mm", element!.Width);
        Assert.Equal(Alignments.Center, element.Alignment);
    }

    // ── Section elements ── Spacer ────────────────────────────────────────

    [Fact]
    public void SectionBuilder_Spacer_CreatesEmptyText()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.Spacer()))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as TextElement;
        Assert.Equal("", element!.Text);
    }

    // ── Section ── Height & Background ────────────────────────────────────

    [Fact]
    public void SectionBuilder_HeightAndBackground()
    {
        var template = Pdf.Create()
            .AddPage(p => p.Header(h => h.Height("40mm").Background("#003366").Text("Header")))
            .Build();

        var header = template.Pages[0].Header;
        Assert.Equal("40mm", header!.Height);
        Assert.Equal("#003366", header.Background);
    }

    // ── Section ── Theme defaults for header/footer height ────────────────

    [Fact]
    public void PageBuilder_Header_AppliesThemeHeight()
    {
        var template = Pdf.Create()
            .WithTheme(PdfTheme.Default)
            .AddPage(p => p.Header(h => h.Text("Header")))
            .Build();

        Assert.Equal("25mm", template.Pages[0].Header!.Height);
    }

    [Fact]
    public void PageBuilder_Footer_AppliesThemeHeight()
    {
        var template = Pdf.Create()
            .WithTheme(PdfTheme.Default)
            .AddPage(p => p
                .Body(b => b.Text("x"))
                .Footer(f => f.Text("Footer")))
            .Build();

        Assert.Equal("15mm", template.Pages[0].Footer!.Height);
    }

    // ── Complex composite ────────────────────────────────────────────────

    [Fact]
    public void FullDocument_BuildsCorrectStructure()
    {
        var template = Pdf.Create()
            .WithTheme(PdfTheme.Default)
            .WithMetadata(m => m.Title("Report").Author("Dev"))
            .AddPage(p => p
                .Header(h => h.Text("Company Name").Line())
                .Body(b => b
                    .Heading1("Chapter 1")
                    .Paragraph("Introduction text...")
                    .Spacer()
                    .Table(t => t
                        .Column("Item", "60%")
                        .Column("Price", "40%", Alignments.Right)
                        .Row("Widget", "$10")
                        .Row("Gadget", "$20"))
                    .Line()
                    .Text("Total: $30", o => o.Alignment = Alignments.Right))
                .Footer(f => f.Line().CaptionText("Page {pageNumber} of {pageCount}")))
            .AddPage(p => p
                .Body(b => b.Heading1("Chapter 2").Paragraph("Content...")))
            .Build();

        Assert.Equal("Report", template.Metadata?.Title);
        Assert.Equal(2, template.Pages.Count);

        // Page 1
        var p1 = template.Pages[0];
        Assert.NotNull(p1.Header);
        Assert.Equal(2, p1.Header!.Elements.Count); // text + line
        Assert.Equal(6, p1.Body.Elements.Count); // h1 + para + spacer + table + line + text
        Assert.NotNull(p1.Footer);
        Assert.Equal(2, p1.Footer!.Elements.Count); // line + caption

        // Page 2
        var p2 = template.Pages[1];
        Assert.Null(p2.Header);
        Assert.Equal(2, p2.Body.Elements.Count); // h1 + para
        Assert.Null(p2.Footer);

        // Verify theme styles present
        Assert.Contains(ThemeStyleNames.Heading1, template.Styles.Keys);
    }

    // ── Element method ───────────────────────────────────────────────────

    [Fact]
    public void SectionBuilder_Element_AddsRawElement()
    {
        var custom = new TextElement { Text = "Raw", FontSize = 99 };

        var template = Pdf.Create()
            .AddPage(p => p.Body(b => b.Element(custom)))
            .Build();

        var element = template.Pages[0].Body.Elements[0] as TextElement;
        Assert.Equal(99d, element!.FontSize);
    }
}