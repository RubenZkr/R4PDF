# Getting Started with R4PDF Fluent API

Welcome! This guide will help you get up and running with R4PDF's fluent builder API in just a few minutes.

## Installation

Add R4PDF to your .NET project:

```bash
dotnet add package R4PDF
```

Or add it manually to your `.csproj`:

```xml
<ItemGroup>
    <PackageReference Include="R4PDF" Version="1.0.4" />
</ItemGroup>
```

## Your First PDF

Here's the simplest possible example:

```csharp
using R4PDF.Fluent;

// Create a simple PDF
var pdf = Pdf.Create()
    .AddPage(page => page
        .Body(body => body
            .Heading1("Hello, World!")
            .Text("My first PDF with R4PDF")
        )
    )
    .Generate();

// Save it
File.WriteAllBytes("hello.pdf", pdf);
```

That's it! You now have a PDF file.

## Adding a Theme

Themes provide consistent styling across your document. R4PDF comes with three beautiful themes:

```csharp
var pdf = Pdf.Create()
    .WithTheme(PdfTheme.Default)  // Try: .Dark or .Modern
    .AddPage(page => page
        .Body(body => body
            .Heading1("Professional PDF")
            .Text("With styling applied automatically")
        )
    )
    .Generate();

File.WriteAllBytes("styled.pdf", pdf);
```

See the difference? The theme handles colors, fonts, and spacing.

## Multi-Page Document

Adding more pages is simple:

```csharp
var pdf = Pdf.Create()
    .WithTheme(PdfTheme.Modern)
    .AddPage(page => page
        .Body(body => body
            .Heading1("Page 1")
            .Paragraph("First page content...")
        )
    )
    .AddPage(page => page
        .Body(body => body
            .Heading1("Page 2")
            .Paragraph("Second page content...")
        )
    )
    .AddPage(page => page
        .Body(body => body
            .Heading1("Page 3")
            .Paragraph("Third page content...")
        )
    )
    .Generate();

File.WriteAllBytes("multi_page.pdf", pdf);
```

## Page Headers and Footers

Add consistent headers and footers to every page:

```csharp
var pdf = Pdf.Create()
    .WithTheme(PdfTheme.Default)
    .AddPage(page => page
        .Header(h => h
            .Text("Company Name")
            .MutedText()
        )
        .Body(b => b
            .Heading1("Report")
            .Paragraph("Main content here...")
        )
        .Footer(f => f
            .Text($"Page {page.PageNumber} - Generated: {DateTime.Now:MMMM d, yyyy}")
            .CaptionText()
        )
    )
    .Generate();

File.WriteAllBytes("with_header_footer.pdf", pdf);
```

## Adding Tables

Tables are perfect for structured data:

```csharp
var pdf = Pdf.Create()
    .WithTheme(PdfTheme.Default)
    .AddPage(page => page
        .Body(body => body
            .Heading1("Sales Data")
            .Table(table => table
                .Column("Product")
                .Column("Q1")
                .Column("Q2")
                .Column("Q3")
                .Row("Widget A", "$10,000", "$12,000", "$15,000")
                .Row("Widget B", "$8,000", "$9,000", "$11,000")
                .Row("Widget C", "$5,000", "$6,000", "$8,000")
            )
        )
    )
    .Generate();

File.WriteAllBytes("table.pdf", pdf);
```

## Styling Text

Control how text looks with inline styling:

```csharp
var pdf = Pdf.Create()
    .AddPage(page => page
        .Body(body => body
            .Heading1("Text Styling")
            .Text("Regular text")
            .Text(t => t
                .Text("Custom text with blue color")
                .Color("#0066CC")
            )
            .Paragraph("A paragraph with more generous line height")
            .AccentText("Important!")
            .MutedText("Subtle information")
            .CaptionText("Small caption")
        )
    )
    .Generate();

File.WriteAllBytes("styled_text.pdf", pdf);
```

## Document Metadata

Add document information (visible in PDF properties):

```csharp
var pdf = Pdf.Create()
    .WithMetadata(meta => meta
        .Title("My Important Document")
        .Author("Your Name")
        .Subject("Project Report")
        .Keywords("report, quarterly, 2026")
    )
    .AddPage(page => page
        .Body(body => body
            .Heading1("Documented PDF")
            .Text("Check the document properties to see the metadata")
        )
    )
    .Generate();

File.WriteAllBytes("with_metadata.pdf", pdf);
```

## Page Layout Control

Customize page size, orientation, and margins:

```csharp
var pdf = Pdf.Create()
    .AddPage(page => page
        .Settings(s => s
            .PageSize("A4")              // Also: "Letter", "A3", "Legal", etc.
            .Orientation("Portrait")     // or "Landscape"
            .Margins(
                top: "20mm",
                right: "15mm",
                bottom: "20mm",
                left: "15mm"
            )
        )
        .Body(body => body
            .Heading1("Custom Layout")
            .Paragraph("This page has custom margins and orientation")
        )
    )
    .Generate();

File.WriteAllBytes("custom_layout.pdf", pdf);
```

## Spacing and Layout

Control whitespace with spacers:

```csharp
var pdf = Pdf.Create()
    .AddPage(page => page
        .Body(body => body
            .Heading1("Section 1")
            .Paragraph("Some content...")
            .Spacer("20mm")              // Add 20mm vertical space
            .Heading2("Section 2")
            .Paragraph("More content...")
            .Spacer("10mm")
            .Heading3("Subsection")
            .Paragraph("Even more content...")
        )
    )
    .Generate();

File.WriteAllBytes("spaced.pdf", pdf);
```

## Custom Colors and Formatting

Use hex color codes for precise control:

```csharp
var pdf = Pdf.Create()
    .AddPage(page => page
        .Body(body => body
            .Heading1("Colorful Content")
            .Text(t => t
                .Text("Red text")
                .Color("#FF0000")
            )
            .Text(t => t
                .Text("Green text")
                .Color("#00AA00")
            )
            .Text(t => t
                .Text("Blue text")
                .Color("#0000FF")
            )
            .Text(t => t
                .Text("Large, bold, centered")
                .FontSize(20)
                .FontWeight("bold")
                .Alignment("center")
                .Color("#0066CC")
            )
        )
    )
    .Generate();

File.WriteAllBytes("colored.pdf", pdf);
```

## Save to File vs Stream

Generate PDF bytes and save how you like:

```csharp
// Save to file directly
Pdf.Create()
    .AddPage(page => page
        .Body(body => body.Text("Content"))
    )
    .GenerateToFile("output.pdf");

// Get bytes
var pdfBytes = Pdf.Create()
    .AddPage(page => page
        .Body(body => body.Text("Content"))
    )
    .Generate();

File.WriteAllBytes("output.pdf", pdfBytes);

// Write to stream
using var stream = new MemoryStream();
Pdf.Create()
    .AddPage(page => page
        .Body(body => body.Text("Content"))
    )
    .GenerateToStream(stream);

// Use stream for download, email, etc.
byte[] pdfData = stream.ToArray();
```

## Combining Everything

Here's a more complete example combining several features:

```csharp
using R4PDF.Fluent;

public byte[] GenerateReport(string customerName, List<string> items)
{
    var itemCount = items.Count;
    
    return Pdf.Create()
        .WithTheme(PdfTheme.Default)
        .WithMetadata(meta => meta
            .Title($"Order Confirmation - {customerName}")
            .Author("E-Commerce System")
        )
        .AddPage(page => page
            .Settings(s => s
                .PageSize("A4")
                .Margins("20mm", "15mm", "20mm", "15mm")
            )
            .Header(h => h
                .Text("ORDER CONFIRMATION")
                .AccentText()
            )
            .Body(b => b
                .Heading1("Thank You for Your Order!")
                .Paragraph($"Dear {customerName},")
                .Spacer("5mm")
                .Paragraph("Your order has been confirmed and will be processed shortly.")
                .Spacer("15mm")
                
                .Heading2("Order Summary")
                .Table(t => t
                    .Column("Item")
                    .Column("Quantity")
                    .Row(items.Select((item, i) => (item, (i + 1).ToString())).ToArray())
                )
                .Spacer("15mm")

                .Text($"Total Items: {itemCount}")
                .Spacer("10mm")

                .Paragraph("Your order will arrive within 3-5 business days. Track your order using the tracking number provided in your confirmation email.")
                .Spacer("10mm")

                .Text("Questions? Contact us at support@example.com")
                .MutedText()
            )
            .Footer(f => f
                .CaptionText($"Generated {DateTime.Now:MMMM d, yyyy}")
            )
        )
        .Generate();
}

// Use it
var pdf = GenerateReport("John Doe", new() { "Widget A", "Gadget B", "Tool C" });
File.WriteAllBytes("order_confirmation.pdf", pdf);
```

## What's Next?

You now know the basics! Explore more:

- **[Fluent API Guide](FLUENT_API.md)** - Complete reference with all methods and options
- **[Themes & Customization](FLUENT_THEMES.md)** - Learn about themes and create custom styles
- **[Real-World Examples](FLUENT_EXAMPLES.md)** - Invoices, reports, letters, and more
- **[JSON Templates](../README.md)** - Original template-based approach

## Common Questions

### Q: Do I need to use a theme?
**A:** No, themes are optional. Use `Pdf.Create()` without `.WithTheme()` and style elements individually.

### Q: Can I use both fluent API and JSON templates?
**A:** Yes! Both approaches work independently. Use the fluent API for code-first generation, JSON templates for data-driven generation.

### Q: What fonts are supported?
**A:** Liberation Sans (default), Liberation Serif, Arial, Georgia, Times New Roman, Courier New, and more.

### Q: Can I generate multiple PDFs in parallel?
**A:** Yes, the API is thread-safe. Each `Pdf.Create()` call is independent.

### Q: How do I add images?
**A:** Use `.Image("path/to/image.png")` in a section. Supports PNG, JPG, and GIF.

### Q: Can I create a PDF from a template and then enhance it?
**A:** The fluent API is for code-first generation. For template-based generation, use the JSON approach (see README.md).

## Troubleshooting

**PDF file is empty or blank**
- Ensure you added at least one `.AddPage()` with content
- Check that `.Generate()` is called
- Verify file paths are correct

**Text doesn't appear**
- Add content to `.Body()`, not just `.Settings()`
- Ensure `AddPage()` is called before `Generate()`

**Font looks different than expected**
- Specify font family explicitly: `.FontFamily("Arial")`
- Not all fonts are available on all systems; stick to common fonts

**Images don't show**
- Verify image file path is correct and file exists
- Ensure image file format is supported (PNG, JPG, GIF)
- Check file permissions

## Getting Help

- [GitHub Issues](https://github.com/RubenZkr/R4PDF/issues)
- [Full API Documentation](FLUENT_API.md)
- [Code Examples](FLUENT_EXAMPLES.md)

Happy PDF generating! 🎉
