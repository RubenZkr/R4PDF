using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using R4PDF.Models.Elements;
using R4PDF.Parsing;
using R4PDF.Rendering;
using Xunit;

namespace R4PDF.Tests;

public class KeepWithNextTests
{
    /// <summary>
    ///     A spacer leaves room for the heading but not for the heading plus the start of the table
    ///     below it, nor for the table alone.
    /// </summary>
    private static string Template(bool keepWithNext) => $$"""
        {
            "settings": { "autoPagination": { "enabled": true } },
            "pages": [{
                "body": {
                    "elements": [
                        { "type": "paragraph", "content": "x", "fontSize": 1, "spacing": { "after": "247mm" } },
                        { "type": "paragraph", "content": "Kop", "fontSize": 12, "keepWithNext": {{(keepWithNext ? "true" : "false")}} },
                        {
                            "type": "table",
                            "columns": [{ "name": "A", "width": "100%" }],
                            "rows": [{ "cells": ["Rij"] }]
                        }
                    ]
                }
            }]
        }
        """;

    [Fact]
    public void KeepWithNext_HeadingThatFitsWithoutItsTable_MovesToTheNextPage()
    {
        var overflow = RenderFirstPage(Template(keepWithNext: true));

        var heading = Assert.IsType<ParagraphElement>(overflow[0]);
        Assert.Equal("Kop", heading.Content);
        Assert.True(heading.KeepWithNext, "the flag survives the move, so the heading keeps with next on the next page too");
        Assert.IsType<TableElement>(overflow[1]);
    }

    [Fact]
    public void WithoutKeepWithNext_TheHeadingStaysAndOnlyTheTableMoves()
    {
        var overflow = RenderFirstPage(Template(keepWithNext: false));

        Assert.Single(overflow);
        Assert.IsType<TableElement>(overflow[0]);
    }

    [Fact]
    public void KeepWithNext_OnAnEmptyPage_RendersAnyway()
    {
        var template = """
            {
                "settings": { "autoPagination": { "enabled": true } },
                "pages": [{ "body": { "elements": [
                    { "type": "paragraph", "content": "Kop", "keepWithNext": true },
                    { "type": "paragraph", "content": "Tekst" }
                ] } }]
            }
            """;

        Assert.True(new PdfGenerator().Generate(template).Length > 0);
        Assert.Empty(RenderFirstPage(template));
    }

    private static List<PdfElement> RenderFirstPage(string json)
    {
        var template = TemplateParser.Parse(json);
        using var document = new PdfDocument();
        var page = document.AddPage();
        page.Size = PdfSharpCore.PageSize.A4;
        using var gfx = XGraphics.FromPdfPage(page);

        var result = new PageRenderer(new StyleResolver(template.Styles)).RenderWithAutoPagination(
            gfx, template.Pages[0], template.Settings, 1, 1, template.Settings.AutoPagination);

        return result.OverflowBody?.Elements ?? new List<PdfElement>();
    }
}
