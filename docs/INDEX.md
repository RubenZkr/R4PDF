# R4PDF Fluent API Documentation

Welcome to the R4PDF Fluent API documentation. This section covers the modern, code-first approach to PDF generation.

## 📚 Documentation Structure

### For First-Time Users
Start here if you're new to R4PDF:
- **[Getting Started](GETTING_STARTED.md)** - Quick introduction with simple examples
- **5 minutes** to your first PDF

### Core Documentation
- **[Fluent API Guide](FLUENT_API.md)** - Complete API reference with all methods and options
- **[Themes & Customization](FLUENT_THEMES.md)** - Built-in themes and custom styling
- **[Code Examples](FLUENT_EXAMPLES.md)** - Real-world use cases and patterns

---

## Quick Navigation

### By Use Case

| What do you want to do? | Documentation |
|------------------------|---------------|
| Generate your first PDF | [Getting Started](GETTING_STARTED.md) |
| Build a multi-page document | [Fluent API Guide](FLUENT_API.md#multi-page-documents) |
| Create a professional invoice | [Code Examples](FLUENT_EXAMPLES.md#invoice) |
| Generate a business report | [Code Examples](FLUENT_EXAMPLES.md#business-report) |
| Add tables and data | [Fluent API Guide](FLUENT_API.md#table-builder) |
| Style text and colors | [Fluent API Guide](FLUENT_API.md#text-styling) |
| Use themes | [Themes & Customization](FLUENT_THEMES.md) |
| Create a custom theme | [Themes & Customization](FLUENT_THEMES.md#custom-themes) |
| Add headers/footers | [Fluent API Guide](FLUENT_API.md#page-builder) |
| Control page layout | [Fluent API Guide](FLUENT_API.md#settings-builder) |

### By Component

| Component | Learn About |
|-----------|------------|
| Document Builder | [Fluent API Guide](FLUENT_API.md#document-builder) |
| Page Builder | [Fluent API Guide](FLUENT_API.md#page-builder) |
| Section Builder | [Fluent API Guide](FLUENT_API.md#section-builder) |
| Table Builder | [Fluent API Guide](FLUENT_API.md#table-builder) |
| Settings Builder | [Fluent API Guide](FLUENT_API.md#settings-builder) |
| Metadata Builder | [Fluent API Guide](FLUENT_API.md#metadata-builder) |
| TextOptions | [Fluent API Guide](FLUENT_API.md#textoptions) |
| ParagraphOptions | [Fluent API Guide](FLUENT_API.md#paragraphoptions) |

---

## Examples at a Glance

### Simple PDF
```csharp
var pdf = Pdf.Create()
    .WithTheme(PdfTheme.Default)
    .AddPage(page => page
        .Body(b => b
            .Heading1("Hello, World!")
            .Text("My first PDF")
        )
    )
    .Generate();

File.WriteAllBytes("hello.pdf", pdf);
```

### Professional Invoice
See [Code Examples - Invoice](FLUENT_EXAMPLES.md#invoice) for a complete invoice example with:
- Item tables
- Calculated totals
- Professional formatting

### Business Report
See [Code Examples - Business Report](FLUENT_EXAMPLES.md#business-report) for a complete report with:
- Multi-section content
- Key metrics tables
- Custom styling

### Data Export
See [Code Examples - Data Table Export](FLUENT_EXAMPLES.md#data-table-export) for exporting lists to PDF tables.

---

## Theme Quick Reference

### Built-In Themes

| Theme | Best For | Colors | Typography |
|-------|----------|--------|-----------|
| **Default** | Business, Reports | Blues & Grays | Professional |
| **Dark** | Modern, Tech | Dark bg, Light text | Contemporary |
| **Modern** | Minimalist, Sleek | Slate gray | Clean, spacious |

```csharp
.WithTheme(PdfTheme.Default)  // or .Dark or .Modern
```

See [Themes & Customization](FLUENT_THEMES.md) for details.

---

## Feature Highlights

✨ **Fluent Builder Pattern**
- Chainable, readable syntax
- Type-safe C# code
- IntelliSense support

📄 **Multi-Page Documents**
- Built-in page management
- Headers and footers
- Custom page settings
- Multiple sections per page

🎨 **Themes & Styling**
- 3 beautiful built-in themes
- Custom theme support
- Named styles
- Inline overrides

📊 **Rich Content**
- Text with formatting (bold, color, size)
- Tables with customization
- Images (PNG, JPG, GIF)
- Lines and rectangles
- Flexible spacing

📐 **Page Layout**
- Multiple page sizes (A4, Letter, A3, etc.)
- Portrait/Landscape
- Custom margins
- Header/footer sections

---

## Comparison: Fluent API vs JSON Templates

### Fluent API (Code-First)
✅ Best for:
- Dynamic content
- Complex logic
- Code-based workflows
- Real-time generation

```csharp
Pdf.Create()
    .WithTheme(PdfTheme.Default)
    .AddPage(page => page
        .Body(b => b.Heading1("Title"))
    )
    .Generate();
```

### JSON Templates (Data-Driven)
✅ Best for:
- Design-first approach
- Configuration-based styling
- Template reusability
- Non-programmers designing PDFs

See [Main README](../README.md) for JSON approach.

**Can I use both?** Yes! They work independently.

---

## Supported Page Sizes

- **A-Series**: A3, A4, A5, A6
- **B-Series**: B4, B5, B6
- **Standard**: Letter, Legal
- **Custom**: Specify width and height

---

## Supported Units

- `mm` - Millimeters (default)
- `cm` - Centimeters
- `in` - Inches (e.g., "1in")
- `pt` - Points (e.g., "12pt")

```csharp
.Spacer("20mm")
.Width("10cm")
.Margins("1in")
```

---

## FAQ

**Q: Is R4PDF free?**
A: Yes, open-source MIT license. See LICENSE file.

**Q: What .NET versions are supported?**
A: .NET 8.0, 9.0, and 10.0.

**Q: Can I use R4PDF in production?**
A: Yes, it's production-ready. See the GitHub repository for issues and discussions.

**Q: How do I report a bug?**
A: File an issue on [GitHub](https://github.com/RubenZkr/R4PDF/issues).

**Q: Can I contribute?**
A: Yes! Fork the repository and submit pull requests.

---

## Related Resources

- [R4PDF GitHub Repository](https://github.com/RubenZkr/R4PDF)
- [Main README](../README.md) - Installation and overview
- [Original JSON API](../README.md#json-templates) - Data-driven approach
- [NuGet Package](https://www.nuget.org/packages/R4PDF)

---

## Getting Help

| Need Help With | Resource |
|----------------|----------|
| Getting started | [Getting Started Guide](GETTING_STARTED.md) |
| Specific API | [Fluent API Guide](FLUENT_API.md) |
| Code examples | [Examples](FLUENT_EXAMPLES.md) |
| Themes | [Themes Guide](FLUENT_THEMES.md) |
| Bugs/Issues | [GitHub Issues](https://github.com/RubenZkr/R4PDF/issues) |

---

## Latest Updates

**Version 1.0.4** ✨
- Complete fluent API implementation
- 3 professional themes (Default, Dark, Modern)
- Theme customization support
- Multi-page documents with headers/footers
- Rich text styling
- Table support with formatting
- 111 comprehensive tests

---

**Ready to get started?** → [Getting Started Guide](GETTING_STARTED.md) ⚡
