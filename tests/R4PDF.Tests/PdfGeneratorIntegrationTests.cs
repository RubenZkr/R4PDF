namespace R4PDF.Tests;

public class PdfGeneratorIntegrationTests
{
    private readonly PdfGenerator _generator = new();

    [Fact]
    public void Generate_SimpleText_ProducesPdf()
    {
        var template = """
                       {
                           "pages": [{
                               "body": {
                                   "elements": [
                                       { "type": "text", "text": "Hello, World!", "fontSize": 24, "fontWeight": "bold" }
                                   ]
                               }
                           }]
                       }
                       """;

        var pdf = _generator.Generate(template);

        Assert.NotNull(pdf);
        Assert.True(pdf.Length > 0);
        // PDF files start with %PDF
        Assert.Equal(0x25, pdf[0]); // %
        Assert.Equal(0x50, pdf[1]); // P
        Assert.Equal(0x44, pdf[2]); // D
        Assert.Equal(0x46, pdf[3]); // F
    }

    [Fact]
    public void Generate_WithStyles_ProducesPdf()
    {
        var template = """
                       {
                           "styles": {
                               "heading": { "fontSize": 20, "fontWeight": "bold", "color": "#003366" },
                               "body": { "fontSize": 11, "color": "#333333" }
                           },
                           "pages": [{
                               "body": {
                                   "elements": [
                                       { "type": "text", "text": "Report Title", "style": "heading" },
                                       { "type": "paragraph", "content": "This is the report body text with multiple words that should wrap if needed.", "style": "body" }
                                   ]
                               }
                           }]
                       }
                       """;

        var pdf = _generator.Generate(template);
        Assert.True(pdf.Length > 0);
    }

    [Fact]
    public void Generate_WithTable_ProducesPdf()
    {
        var template = """
                       {
                           "pages": [{
                               "body": {
                                   "elements": [
                                       { "type": "text", "text": "Invoice", "fontSize": 24, "fontWeight": "bold" },
                                       {
                                           "type": "table",
                                           "columns": [
                                               { "name": "Item", "width": "50%" },
                                               { "name": "Qty", "width": "20%", "alignment": "center" },
                                               { "name": "Price", "width": "30%", "alignment": "right" }
                                           ],
                                           "rows": [
                                               { "cells": ["Widget A", "10", "$5.00"] },
                                               { "cells": ["Widget B", "5", "$12.50"] },
                                               { "cells": ["Widget C", "100", "$0.50"] }
                                           ],
                                           "headerStyle": { "backgroundColor": "#003366", "color": "#FFFFFF", "fontWeight": "bold" },
                                           "alternateRowColors": true,
                                           "alternateColor": "#F5F5F5"
                                       }
                                   ]
                               }
                           }]
                       }
                       """;

        var pdf = _generator.Generate(template);
        Assert.True(pdf.Length > 0);
    }

    [Fact]
    public void Generate_WithDataBinding_ProducesPdf()
    {
        var template = """
                       {
                           "pages": [{
                               "body": {
                                   "elements": [
                                       { "type": "text", "text": "Hello, ${customer.name}!", "fontSize": 18 },
                                       { "type": "text", "text": "Order #${order.id}" },
                                       { "type": "text", "text": "Total: $${order.total}" }
                                   ]
                               }
                           }]
                       }
                       """;

        var data = """
                   {
                       "customer": { "name": "John Doe" },
                       "order": { "id": "12345", "total": "150.00" }
                   }
                   """;

        var pdf = _generator.Generate(template, data);
        Assert.True(pdf.Length > 0);
    }

    [Fact]
    public void Generate_WithHeaderFooter_ProducesPdf()
    {
        var template = """
                       {
                           "pages": [{
                               "header": {
                                   "height": "30mm",
                                   "elements": [
                                       { "type": "text", "text": "Company Name", "fontSize": 16, "fontWeight": "bold", "color": "#003366" }
                                   ]
                               },
                               "body": {
                                   "elements": [
                                       { "type": "text", "text": "Main content goes here." }
                                   ]
                               },
                               "footer": {
                                   "height": "20mm",
                                   "elements": [
                                       { "type": "text", "text": "Page {pageNumber} of {pageCount}", "fontSize": 9, "alignment": "center", "color": "#999999" }
                                   ]
                               }
                           }]
                       }
                       """;

        var pdf = _generator.Generate(template);
        Assert.True(pdf.Length > 0);
    }

    [Fact]
    public void Generate_WithShapes_ProducesPdf()
    {
        var template = """
                       {
                           "pages": [{
                               "body": {
                                   "elements": [
                                       { "type": "rectangle", "fillColor": "#EEEEEE", "strokeColor": "#CCCCCC", "width": "180mm", "height": "30mm" },
                                       { "type": "text", "text": "Content inside a box", "fontSize": 14 },
                                       { "type": "line", "color": "#CCCCCC", "strokeWidth": "1pt" },
                                       { "type": "text", "text": "After the line", "fontSize": 12 }
                                   ]
                               }
                           }]
                       }
                       """;

        var pdf = _generator.Generate(template);
        Assert.True(pdf.Length > 0);
    }

    [Fact]
    public void GenerateToFile_CreatesFileOnDisk()
    {
        var template = """
                       {
                           "pages": [{
                               "body": {
                                   "elements": [
                                       { "type": "text", "text": "File output test" }
                                   ]
                               }
                           }]
                       }
                       """;

        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.pdf");

        try
        {
            _generator.GenerateToFile(template, path);
            Assert.True(File.Exists(path));
            var bytes = File.ReadAllBytes(path);
            Assert.True(bytes.Length > 0);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Generate_MultiplePages_ProducesPdf()
    {
        var template = """
                       {
                           "settings": { "pageSize": "A4", "orientation": "Portrait" },
                           "pages": [
                               {
                                   "body": {
                                       "elements": [
                                           { "type": "text", "text": "Page 1 Content", "fontSize": 20 }
                                       ]
                                   }
                               },
                               {
                                   "settings": { "pageSize": "A4", "orientation": "Landscape" },
                                   "body": {
                                       "elements": [
                                           { "type": "text", "text": "Page 2 - Landscape", "fontSize": 20 }
                                       ]
                                   }
                               }
                           ]
                       }
                       """;

        var pdf = _generator.Generate(template);
        Assert.True(pdf.Length > 0);
    }
}