# R4PDF Fluent API Guide

The fluent API is a modern, code-first approach to PDF generation using a builder pattern. It provides a clean, chainable syntax for constructing PDFs programmatically without JSON templates.

## Installation

```bash
dotnet add package R4PDF
```

## Quick Start

```csharp
using R4PDF;
using R4PDF.Fluent;

// Create and generate a simple PDF
var pdf = Pdf.Create()
    .WithTheme(PdfTheme.Default)
    .AddPage(page => page
        .Body(body => body
            .Heading1("Welcome to R4PDF")
            .Text("Generate beautiful PDFs with C#")
        )
    )
    .Generate();

// Save to file
File.WriteAllBytes("output.pdf", pdf);
```

## Basic Structure

The fluent API follows this hierarchy:

```
Pdf.Create()                    // Start document builder
  ├─ WithTheme()               // Apply a theme (optional)
  ├─ WithMetadata()            // Set document metadata
  ├─ AddPage()                 // Add a page
  │   ├─ Settings()            // Page-specific settings
  │   ├─ Header()              // Page header section
  │   ├─ Body()                // Main content section
  │   └─ Footer()              // Page footer section
  ├─ AddStyle()                // Define custom named styles
  └─ Generate()                // Generate PDF bytes
```

## Core Components

### Document Builder (`PdfDocumentBuilder`)

The root builder for creating PDF documents.

**Methods:**
- `WithTheme(PdfTheme)` - Apply a built-in or custom theme
- `WithMetadata(Action<MetadataBuilder>)` - Configure document metadata
- `AddPage(Action<PageBuilder>)` - Add a new page
- `AddStyle(string name, PdfStyle)` - Add a custom named style
- `Generate()` - Returns `byte[]` - Generate PDF bytes
- `GenerateToStream(Stream)` - Write PDF to a stream
- `GenerateToFile(string path)` - Save PDF to a file

**Example:**
```csharp
var pdf = Pdf.Create()
    .WithTheme(PdfTheme.Default)
    .WithMetadata(meta => meta
        .Title("Invoice")
        .Author("Company Name")
    )
    .AddPage(page => { /* ... */ })
    .AddPage(page => { /* ... */ })
    .GenerateToFile("document.pdf");
```

### Metadata Builder (`MetadataBuilder`)

Configure document properties.

**Methods:**
- `Title(string)` - Set document title
- `Author(string)` - Set document author
- `Subject(string)` - Set document subject
- `Keywords(string)` - Set document keywords

**Example:**
```csharp
.WithMetadata(meta => meta
    .Title("Monthly Report")
    .Author("Analytics Team")
    .Subject("Sales Performance")
    .Keywords("report, sales, q1 2026")
)
```

### Page Builder (`PageBuilder`)

Define the structure of a single page.

**Methods:**
- `Settings(Action<SettingsBuilder>)` - Configure page dimensions and margins
- `Header(Action<SectionBuilder>)` - Add page header
- `Body(Action<SectionBuilder>)` - Add main content
- `Footer(Action<SectionBuilder>)` - Add page footer

**Example:**
```csharp
.AddPage(page => page
    .Settings(s => s
        .PageSize("A4")
        .Orientation("Portrait")
        .Margins("20mm", "20mm", "15mm", "15mm")
    )
    .Header(h => h.Text("© 2026 Company Name").MutedText())
    .Body(b => { /* content */ })
    .Footer(f => f.Text("Page 1").CaptionText())
)
```

### Settings Builder (`SettingsBuilder`)

Configure page layout and dimensions.

**Methods:**
- `PageSize(string)` - Set page size (e.g., "A4", "Letter", "A3")
- `Orientation(string)` - Set orientation ("Portrait" or "Landscape")
- `Margins(string top, string right, string bottom, string left)` - Set margins

**Supported Page Sizes:**
- `A3`, `A4`, `A5`, `A6`
- `Letter`, `Legal`
- `B4`, `B5`, `B6`

**Unit Support:**
- `mm` - Millimeters (default)
- `cm` - Centimeters
- `in` - Inches
- `pt` - Points

**Example:**
```csharp
.Settings(s => s
    .PageSize("A4")
    .Orientation("Landscape")
    .Margins("25mm", "20mm", "25mm", "20mm")
)
```

### Section Builder (`SectionBuilder`)

Build content for headers, bodies, and footers. Supports text, tables, images, lines, and rectangles.

**Text Methods:**
- `Heading1(string text)` - Large heading (24pt)
- `Heading2(string text)` - Medium heading (18pt)
- `Heading3(string text)` - Small heading (14pt)
- `Text(string text)` - Regular text (11pt)
- `Paragraph(string text)` - Paragraph with line height
- `AccentText(string text)` - Bold, accent color
- `MutedText(string text)` - Muted gray text
- `CaptionText(string text)` - Small caption text (9pt)

**Advanced Text:**
- `Text(Action<TextOptions>)` - Customize text style
- `Paragraph(Action<ParagraphOptions>)` - Customize paragraph

**Container Methods:**
- `Table(Action<TableBuilder>)` - Add a table
- `Line(Action<LineOptions>)` - Add a horizontal line
- `Rectangle(Action<RectangleOptions>)` - Add a rectangle
- `Image(string path, Action<ImageOptions>)` - Add an image
- `Element(PdfElement)` - Add a generic element

**Spacing:**
- `Spacer(string height)` - Add vertical spacing

**Example:**
```csharp
.Body(body => body
    .Heading1("Invoice #2026-001")
    .Text("Invoice Date: March 18, 2026")
    .Spacer("10mm")
    .Paragraph("Lorem ipsum dolor sit amet...")
    .Line(l => l.Color("#CCCCCC"))
    .Spacer("5mm")
    .Table(t => { /* ... */ })
)
```

### Table Builder (`TableBuilder`)

Create structured data tables.

**Methods:**
- `Column(string header)` - Add a column with header
- `Row(params string[] values)` - Add a data row
- `HeaderStyle(PdfStyle)` - Customize header style
- `AlternateRowColor(string color)` - Set alternating row color
- `Borders(string width, string color)` - Configure borders
- `ShowHeader(bool)` - Toggle header visibility

**Example:**
```csharp
.Table(t => t
    .Column("Product")
    .Column("Quantity")
    .Column("Price")
    .Row("Widget A", "5", "$50.00")
    .Row("Widget B", "3", "$75.00")
    .Row("Widget C", "2", "$100.00")
    .Borders("0.5pt", "#CCCCCC")
)
```

## Text Styling

### TextOptions

Fine-grained control over text appearance.

```csharp
.Text(t => t
    .FontFamily("Arial")
    .FontSize(14)
    .FontWeight("bold")
    .Color("#0066CC")
    .Alignment("center")
)
```

**Properties:**
- `FontFamily()` - Font name
- `FontSize()` - Font size in points
- `FontWeight()` - "normal" or "bold"
- `Color()` - Hex color code
- `Alignment()` - "left", "center", "right"
- `Style()` - PdfStyle object

### ParagraphOptions

Extended text styling with line height.

```csharp
.Paragraph(p => p
    .FontFamily("Georgia")
    .FontSize(12)
    .Color("#333333")
    .LineHeight(1.5)
)
```

**Properties:**
- Same as TextOptions, plus:
- `LineHeight()` - Line spacing multiplier

### LineOptions

Horizontal line styling.

```csharp
.Line(l => l
    .Color("#CCCCCC")
    .StrokeWidth("1pt")
)
```

**Properties:**
- `Color()` - Hex color code
- `StrokeWidth()` - Line thickness

### RectangleOptions

Rectangle shape styling.

```csharp
.Rectangle(r => r
    .Width("100mm")
    .Height("50mm")
    .FillColor("#F0F0F0")
    .StrokeColor("#333333")
    .StrokeWidth("1pt")
    .CornerRadius("5mm")
)
```

**Properties:**
- `Width()`, `Height()` - Dimensions
- `FillColor()` - Background color
- `StrokeColor()` - Border color
- `StrokeWidth()` - Border thickness
- `CornerRadius()` - Rounded corners

### ImageOptions

Image embedding and sizing.

```csharp
.Image("logo.png", i => i
    .Width("50mm")
    .Height("20mm")
    .Alignment("center")
    .MaintainAspectRatio(true)
)
```

**Properties:**
- `Width()`, `Height()` - Dimensions
- `Alignment()` - "left", "center", "right"
- `MaintainAspectRatio()` - Keep aspect ratio

## Multi-Page Documents

```csharp
var pdf = Pdf.Create()
    .WithTheme(PdfTheme.Default)
    .AddPage(page => page
        .Body(b => b
            .Heading1("Page 1")
            .Text("First page content")
        )
    )
    .AddPage(page => page
        .Body(b => b
            .Heading1("Page 2")
            .Text("Second page content")
        )
    )
    .AddPage(page => page
        .Body(b => b
            .Heading1("Page 3")
            .Text("Third page content")
        )
    )
    .Generate();
```

## Custom Styles

Register reusable named styles at the document level:

```csharp
var pdf = Pdf.Create()
    .AddStyle("subtitle", new PdfStyle
    {
        FontFamily = "Liberation Sans",
        FontSize = 14,
        Color = "#666666",
        Alignment = "center"
    })
    .AddStyle("highlight", new PdfStyle
    {
        FontFamily = "Liberation Sans",
        FontSize = 12,
        FontWeight = "bold",
        Color = "#FF0000"
    })
    .AddPage(page => page
        .Body(b => b
            .Heading1("Report")
            .Element(new TextElement
            {
                Text = "Quarterly Summary",
                Style = "subtitle"
            })
        )
    )
    .Generate();
```

## Complete Example

```csharp
using R4PDF;
using R4PDF.Fluent;

var pdf = Pdf.Create()
    .WithTheme(PdfTheme.Default)
    .WithMetadata(meta => meta
        .Title("Sales Report Q1 2026")
        .Author("Sales Department")
    )
    .AddPage(page => page
        .Settings(s => s
            .PageSize("A4")
            .Margins("20mm", "15mm", "20mm", "15mm")
        )
        .Header(h => h
            .Text("Sales Report - Q1 2026")
            .MutedText()
        )
        .Body(b => b
            .Heading1("Q1 2026 Performance")
            .Spacer("5mm")
            .Text("Report Date: March 18, 2026")
            .Spacer("10mm")
            .Paragraph("The first quarter showed strong performance across all regions...")
            .Spacer("10mm")
            .Table(t => t
                .Column("Region")
                .Column("Revenue")
                .Column("Growth")
                .Row("North", "$250,000", "+15%")
                .Row("South", "$180,000", "+8%")
                .Row("East", "$320,000", "+22%")
                .Row("West", "$210,000", "+12%")
            )
        )
        .Footer(f => f
            .CaptionText("© 2026 Company. Confidential.")
        )
    )
    .GenerateToFile("q1_report.pdf");
```

## See Also

- [Themes & Customization](FLUENT_THEMES.md) - Learn about themes
- [Code Examples](FLUENT_EXAMPLES.md) - More real-world examples
- [JSON Templates](../README.md) - Original JSON-based approach
