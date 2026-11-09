# R4PDF

A free, open-source .NET 8, 9. 10 library that converts JSON templates into fully customizable PDFs. No paid licenses —
just define your layout in JSON and generate PDFs.

## Links

- Repository: https://github.com/RubenZkr/R4PDF
- Issues: https://github.com/RubenZkr/R4PDF/issues

## Installation

```bash
# From a local .nupkg
dotnet add package R4PDF --source ./nupkg

# Or add a project reference
dotnet add reference path/to/R4PDF.csproj
```

## Quick Start

```csharp
using R4PDF;

var generator = new PdfGenerator();

var template = """
{
    "pages": [{
        "body": {
            "elements": [
                { "type": "text", "text": "Hello, World!", "fontSize": 24, "fontWeight": "bold" },
                { "type": "paragraph", "content": "This PDF was generated from a JSON template." }
            ]
        }
    }]
}
""";

// Generate as byte array
byte[] pdf = generator.Generate(template);

// Or save directly to file
generator.GenerateToFile(template, "output.pdf");
```

## Data Binding

Use `${path.to.value}` placeholders in your template, then pass a data JSON object:

```csharp
var template = """
{
    "pages": [{
        "body": {
            "elements": [
                { "type": "text", "text": "Hello, ${customer.name}!" },
                { "type": "text", "text": "Order #${order.id}" }
            ]
        }
    }]
}
""";

var data = """
{
    "customer": { "name": "John Doe" },
    "order": { "id": "12345" }
}
""";

byte[] pdf = generator.Generate(template, data);
```

## JSON Template Schema

### Root Structure

```json
{
    "metadata": { "title": "...", "author": "...", "subject": "...", "keywords": "..." },
    "settings": {
        "pageSize": "A4",
        "orientation": "Portrait",
        "margins": { "top": "20mm", "bottom": "20mm", "left": "15mm", "right": "15mm" }
    },
    "styles": {
        "styleName": { "fontSize": 14, "fontWeight": "bold", "color": "#003366" }
    },
    "pages": [ ... ]
}
```

### Page Structure

```json
{
    "settings": { },
    "header": { "height": "30mm", "elements": [ ... ] },
    "body": { "elements": [ ... ] },
    "footer": { "height": "20mm", "elements": [ ... ] }
}
```

### Element Types

#### Text

Single-line text with optional styling.

```json
{
    "type": "text",
    "text": "Hello World",
    "fontSize": 14,
    "fontWeight": "bold",
    "fontFamily": "Helvetica",
    "color": "#000000",
    "alignment": "left",
    "style": "styleName"
}
```

#### Paragraph

Multi-line text with automatic word wrapping.

```json
{
    "type": "paragraph",
    "content": "Long text that will wrap automatically...",
    "fontSize": 11,
    "lineHeight": 1.5,
    "alignment": "justify",
    "spacing": { "before": "6pt", "after": "6pt" }
}
```

#### Table

```json
{
    "type": "table",
    "columns": [
        { "name": "Item", "width": "50%" },
        { "name": "Price", "width": "25%", "alignment": "right" },
        { "name": "Qty", "width": "25%", "alignment": "center" }
    ],
    "rows": [
        { "cells": ["Widget", "$10.00", "5"], "backgroundColor": "#FFFFFF" }
    ],
    "headerStyle": {
        "backgroundColor": "#003366",
        "color": "#FFFFFF",
        "fontWeight": "bold"
    },
    "alternateRowColors": true,
    "alternateColor": "#F5F5F5",
    "borders": { "width": "0.5pt", "color": "#CCCCCC" }
}
```

A row can style single cells over its own style with `cellStyles` (one entry per column, `null` keeps the row style). With `badge` the background is a rounded label around the text, for a status:

```json
{
    "cells": ["WWFT", "Mogelijke match"],
    "cellStyles": [null, { "backgroundColor": "#FEF0E5", "textColor": "#A84500", "isBold": true, "badge": true }]
}
```

A word that is wider than its column is broken inside, so long file names and identifiers stay within the column.

#### Image

```json
{
    "type": "image",
    "source": "data:image/png;base64,iVBOR...",
    "width": "50mm",
    "alignment": "center",
    "maintainAspectRatio": true
}
```

Supports: base64 data URIs, file paths.

#### Line

```json
{
    "type": "line",
    "color": "#CCCCCC",
    "strokeWidth": "1pt",
    "dashPattern": "4,2"
}
```

#### Rectangle

```json
{
    "type": "rectangle",
    "width": "100mm",
    "height": "50mm",
    "fillColor": "#EEEEEE",
    "strokeColor": "#CCCCCC",
    "strokeWidth": "1pt",
    "cornerRadius": "3mm"
}
```

#### Canvas

A drawing area for diagrams. Its elements are positioned relative to the top-left corner of the canvas and do not move the layout cursor; the canvas takes up its `height` (or, without one, down to its lowest element) and moves to the next page as a whole.

```json
{
    "type": "canvas",
    "height": "60mm",
    "elements": [
        { "type": "rectangle", "x": "10mm", "y": "5mm", "width": "40mm", "height": "15mm", "fillColor": "#152A62", "cornerRadius": "2mm" },
        { "type": "paragraph", "x": "10mm", "y": "9mm", "width": "40mm", "content": "Example B.V.", "alignment": "center", "color": "#FFFFFF" },
        { "type": "line", "x1": "30mm", "y1": "20mm", "x2": "30mm", "y2": "40mm", "color": "#445581" }
    ]
}
```

#### Anchors and links

Every element and table row accepts an `anchor` (a name for where it is rendered) and a `link`: `"#name"` jumps to the page of that anchor, any other value is opened as a URL.

```json
{ "type": "paragraph", "content": "Appendix", "anchor": "appendix" }
{ "type": "table", "columns": [{ "name": "Contents" }], "rows": [{ "cells": ["Appendix"], "link": "#appendix" }] }
```

`GenerateWithLayout` reports the page and area of every anchor (see the API reference).

### Styles

Define reusable styles in the `styles` dictionary and reference by name:

```json
{
    "styles": {
        "heading": { "fontSize": 20, "fontWeight": "bold", "color": "#003366" },
        "body": { "fontSize": 11, "color": "#333333", "lineHeight": 1.5 }
    },
    "pages": [{
        "body": {
            "elements": [
                { "type": "text", "text": "Title", "style": "heading" },
                { "type": "paragraph", "content": "...", "style": "body" }
            ]
        }
    }]
}
```

Styles are merged in priority order: **named style → inline style → element properties**.

### Measurement Units

All size/position values accept these units:

- `mm` — millimeters (e.g., `"20mm"`)
- `cm` — centimeters (e.g., `"2.5cm"`)
- `in` — inches (e.g., `"1in"`)
- `pt` — points (e.g., `"72pt"`)
- `px` — pixels at 96dpi (e.g., `"96px"`)

### Page Sizes

`A3`, `A4`, `A5`, `Letter`, `Legal`, `Tabloid`

### Page Number Placeholders

Use in header/footer text:

- `{pageNumber}` — current page number
- `{pageCount}` — total page count

## API Reference

```csharp
var generator = new PdfGenerator();

// Generate to byte array
byte[] pdf = generator.Generate(templateJson);
byte[] pdf = generator.Generate(templateJson, dataJson);

// Generate to stream
generator.GenerateToStream(templateJson, dataJson, outputStream);

// Generate to file
generator.GenerateToFile(templateJson, "output.pdf");
generator.GenerateToFile(templateJson, dataJson, "output.pdf");

// Generate and report where every anchor was rendered (page index and area in points,
// measured from the top-left corner), e.g. to link to it from another document
PdfRenderResult result = generator.GenerateWithLayout(templateJson);
foreach (var anchor in result.Anchors)
    Console.WriteLine($"{anchor.Name}: page {anchor.PageIndex + 1}");
```

## Building

```bash
dotnet build
dotnet test
dotnet pack -c Release -o ./nupkg
```

## License

MIT — free for any use, commercial or personal.
