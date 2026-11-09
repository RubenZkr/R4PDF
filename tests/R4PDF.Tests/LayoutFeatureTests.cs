using System.Text;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using R4PDF.Models.Elements;
using R4PDF.Parsing;
using R4PDF.Rendering;

namespace R4PDF.Tests;

/// <summary>
///     The layout features of 1.2.0: breaking long words, cell styles, anchors and links, and the canvas.
/// </summary>
public class LayoutFeatureTests
{
    private readonly PdfGenerator _generator = new();

    [Fact]
    public void Table_WordWiderThanItsColumn_IsBrokenOverSeveralLines()
    {
        var shortRow = MeasureRow("kort.pdf");
        var longRow = MeasureRow("b11ce8ba-ec6c-4c1e-9f3a-2d7c5e4f1a90_SSO-Aeternus-antwoord-op-verzoek.pdf");

        Assert.True(longRow > shortRow * 2, $"the long name wraps instead of running into the next column ({longRow} vs {shortRow})");
    }

    [Fact]
    public void Paragraph_WordWiderThanTheParagraph_IsBrokenOverSeveralLines()
    {
        var paragraph = new ParagraphElement { Content = new string('x', 200), FontSize = 10, Width = "40mm" };
        using var document = new PdfDocument();
        using var gfx = XGraphics.FromPdfPage(document.AddPage());

        var (full, _) = new ParagraphRenderer().Measure(gfx, paragraph, new StyleResolver(null).Resolve(paragraph), 500);

        Assert.True(full > 10 * 1.2 * 3, "a 200-character word cannot fit on one 40 mm line");
    }

    [Fact]
    public void Table_CellStyles_SurviveAutoPagination()
    {
        var template = TemplateParser.Parse("""
            {
                "pages": [{ "body": { "elements": [{
                    "type": "table",
                    "columns": [{ "name": "A" }, { "name": "B" }],
                    "rows": [{
                        "cells": ["Bron", "Hit"],
                        "anchor": "rij",
                        "cellStyles": [null, { "backgroundColor": "#FDECEA", "textColor": "#B42318", "isBold": true, "badge": true }]
                    }]
                }] } }]
            }
            """);

        var clone = (TableElement)template.Pages[0].Body.Elements[0].Clone();
        var row = clone.Rows[0];

        Assert.Null(row.CellStyles![0]);
        Assert.True(row.CellStyles[1]!.Badge);
        Assert.Equal("#B42318", row.CellStyles[1]!.TextColor);
        Assert.Equal("rij", row.Anchor);
        Assert.NotSame(template.Pages[0].Body.Elements[0], clone);
    }

    [Fact]
    public void Table_BoldCellStyle_RendersInTheBoldFace()
    {
        var pdf = Encoding.ASCII.GetString(_generator.Generate("""
            {
                "settings": { "autoPagination": { "enabled": true } },
                "pages": [{ "body": { "elements": [{
                    "type": "table",
                    "showHeader": false,
                    "columns": [{ "name": "A" }, { "name": "B" }],
                    "rows": [{ "cells": ["Gewoon", "Vet"], "cellStyles": [null, { "isBold": true, "backgroundColor": "#EEEEEE", "badge": true }] }]
                }] } }]
            }
            """));

        Assert.Contains("Bold", pdf);
    }

    [Fact]
    public void GenerateWithLayout_ReportsThePageOfEveryAnchor()
    {
        var rows = string.Join(",", Enumerable.Range(1, 80).Select(i => $$"""{ "cells": ["Rij {{i}}"], "anchor": "rij-{{i}}" }"""));
        var result = _generator.GenerateWithLayout($$"""
            {
                "settings": { "autoPagination": { "enabled": true } },
                "pages": [{ "body": { "elements": [
                    { "type": "paragraph", "content": "Kop", "anchor": "kop" },
                    { "type": "table", "columns": [{ "name": "A" }], "rows": [{{rows}}] }
                ] } }]
            }
            """);

        var heading = Assert.Single(result.Anchors, anchor => anchor.Name == "kop");
        Assert.Equal(0, heading.PageIndex);
        Assert.True(heading.Height > 0);

        var lastRow = Assert.Single(result.Anchors, anchor => anchor.Name == "rij-80");
        Assert.True(lastRow.PageIndex > 0, "the rows that move to a continuation page report that page");
        Assert.Equal(80, result.Anchors.Count(anchor => anchor.Name.StartsWith("rij-")));
    }

    [Fact]
    public void Link_ToAnAnchor_JumpsToItsPage()
    {
        var result = _generator.GenerateWithLayout("""
            {
                "pages": [
                    { "body": { "elements": [{ "type": "table", "columns": [{ "name": "Inhoud" }], "rows": [{ "cells": ["Naar bijlage"], "link": "#bijlage" }] }] } },
                    { "body": { "elements": [{ "type": "paragraph", "content": "Bijlage", "anchor": "bijlage" }] } }
                ]
            }
            """);

        using var document = PdfReader.Open(new MemoryStream(result.Pdf), PdfDocumentOpenMode.Import);
        Assert.Equal(1, document.Pages[0].Annotations.Count);
        Assert.Equal(0, document.Pages[1].Annotations.Count);
        Assert.Equal(1, Assert.Single(result.Anchors).PageIndex);
    }

    [Fact]
    public void Canvas_ElementsDoNotMoveTheCursor_OnlyTheCanvasHeightDoes()
    {
        var result = _generator.GenerateWithLayout("""
            {
                "pages": [{ "body": { "elements": [
                    { "type": "canvas", "height": "50mm", "anchor": "canvas", "elements": [
                        { "type": "rectangle", "x": "10mm", "y": "5mm", "width": "40mm", "height": "15mm", "fillColor": "#152A62", "cornerRadius": "2mm" },
                        { "type": "paragraph", "x": "10mm", "y": "8mm", "width": "40mm", "content": "Voorbeeld B.V.", "alignment": "center", "anchor": "label" },
                        { "type": "line", "x1": "30mm", "y1": "20mm", "x2": "30mm", "y2": "35mm", "color": "#152A62" }
                    ] },
                    { "type": "paragraph", "content": "Onder het canvas", "anchor": "onder" }
                ] } }]
            }
            """);

        var canvas = result.Anchors.Single(anchor => anchor.Name == "canvas");
        var label = result.Anchors.Single(anchor => anchor.Name == "label");
        var below = result.Anchors.Single(anchor => anchor.Name == "onder");

        Assert.Equal(Points(50), canvas.Height, 1);
        Assert.Equal(canvas.X + Points(10), label.X, 1);
        Assert.Equal(canvas.Y + Points(8), label.Y, 1);
        Assert.Equal(canvas.Y + canvas.Height, below.Y, 1);
    }

    [Fact]
    public void Canvas_WithoutHeight_TakesTheHeightOfItsLowestElement()
    {
        var result = _generator.GenerateWithLayout("""
            {
                "pages": [{ "body": { "elements": [
                    { "type": "canvas", "anchor": "canvas", "elements": [
                        { "type": "rectangle", "x": "0mm", "y": "30mm", "width": "10mm", "height": "10mm", "fillColor": "#000000" }
                    ] }
                ] } }]
            }
            """);

        Assert.Equal(Points(40), result.Anchors.Single().Height, 1);
    }

    [Fact]
    public void Table_CellWithRuns_TakesALinePerNewlineInTheTallestFontOfEachLine()
    {
        var oneLine = MeasureRunsRow(new TextRun { Text = "Naam", FontSize = 14, IsBold = true });
        var twoLines = MeasureRunsRow(
            new TextRun { Text = "Naam", FontSize = 14, IsBold = true },
            new TextRun { Text = "  geb. 1 januari 1970", FontSize = 9 },
            new TextRun { Text = "\nProfiel", FontSize = 9 },
            new TextRun { Text = " 17", FontSize = 9, IsBold = true, Color = "#1570C9" });

        Assert.True(twoLines > oneLine * 1.4, $"the newline starts a second line ({twoLines} vs {oneLine})");
    }

    [Fact]
    public void Table_RunsLeaderDotAndBorder_RenderWithAutoPagination()
    {
        var pdf = _generator.Generate("""
            {
                "settings": { "autoPagination": { "enabled": true } },
                "pages": [{ "body": { "elements": [{
                    "type": "table",
                    "showHeader": false,
                    "verticalAlignment": "middle",
                    "columns": [{ "name": "A", "width": "70%" }, { "name": "B", "width": "20%" }, { "name": "C", "width": "10%", "alignment": "right" }],
                    "rows": [{
                        "cells": ["", "Geen hits", "4"],
                        "cellStyles": [
                            { "runs": [{ "text": "Dossier", "fontSize": 15, "isBold": true }, { "text": "  2", "color": "#5B6478" }], "leader": true, "borderBottomColor": "#13285E", "borderBottomWidth": "1.2pt" },
                            { "badge": true, "backgroundColor": "#E2F3E9", "dotColor": "#2E9E5B", "fontSize": 8 },
                            { "isBold": true }
                        ]
                    }]
                }] } }]
            }
            """);

        using var document = PdfReader.Open(new MemoryStream(pdf), PdfDocumentOpenMode.Import);
        Assert.Equal(1, document.PageCount);
        var clone = (TableElement)TemplateParser.Parse("""
            { "pages": [{ "body": { "elements": [{ "type": "table", "verticalAlignment": "middle", "columns": [{ "name": "A" }],
              "rows": [{ "cells": [""], "cellStyles": [{ "runs": [{ "text": "x" }], "leader": true }] }] }] } }] }
            """).Pages[0].Body.Elements[0].Clone();
        Assert.Equal("middle", clone.VerticalAlignment);
        Assert.True(clone.Rows[0].CellStyles![0]!.Leader);
        Assert.Equal("x", clone.Rows[0].CellStyles![0]!.Runs![0].Text);
    }

    private static double MeasureRunsRow(params TextRun[] runs)
    {
        var table = new TableElement
        {
            Columns = { new TableColumn { Name = "A", Width = "100%" } },
            Rows = { new TableRow { Cells = { "" }, CellStyles = new List<TableCellStyle?> { new() { Runs = runs.ToList() } } } },
            ShowHeader = false,
        };

        using var document = new PdfDocument();
        using var gfx = XGraphics.FromPdfPage(document.AddPage());
        return new TableRenderer().Measure(gfx, table, new StyleResolver(null).Resolve(table), 480).Full;
    }

    private static double MeasureRow(string cell)
    {
        var table = new TableElement
        {
            Columns = { new TableColumn { Name = "Bestand", Width = "30%" }, new TableColumn { Name = "Datum", Width = "70%" } },
            Rows = { new TableRow { Cells = { cell, "6 oktober 2026" } } },
            ShowHeader = false,
        };

        using var document = new PdfDocument();
        using var gfx = XGraphics.FromPdfPage(document.AddPage());
        return new TableRenderer().Measure(gfx, table, new StyleResolver(null).Resolve(table), 480).Full;
    }

    private static double Points(double millimetres) => millimetres * 72 / 25.4;
}
