# R4PDF Themes & Customization

R4PDF includes a powerful theme system to maintain consistent styling across your PDFs. Themes define default colors, fonts, and spacing that are automatically applied to all elements.

## Built-In Themes

R4PDF provides three professionally designed themes out of the box.

### Default Theme

A clean, professional light theme with blues and grays. Perfect for business documents and reports.

**Colors:**
- Primary: `#003366` (dark blue)
- Text: `#333333` (dark gray)
- Accents: `#e94560` (coral red)
- Muted: `#888888` (light gray)

**Typography:**
- Font: Liberation Sans
- Heading 1: 24pt bold, dark blue
- Heading 2: 18pt bold
- Heading 3: 14pt bold
- Body: 11pt, dark gray
- Paragraph: 11pt with 1.5 line height

**Example:**
```csharp
var pdf = Pdf.Create()
    .WithTheme(PdfTheme.Default)
    .AddPage(page => page
        .Body(b => b
            .Heading1("Professional Report")
            .Text("Using the Default theme")
        )
    )
    .Generate();
```

### Dark Theme

A modern theme with a dark background aesthetic and light, vibrant text colors. Great for modern, tech-focused documents.

**Colors:**
- Primary: `#1565C0` (bright blue)
- Text: `#E0E0E0` (light gray)
- Headings: `#64B5F6` to `#BBDEFB` (light blues)
- Accents: `#FF7043` (orange)
- Muted: `#9E9E9E` (medium gray)

**Typography:**
- Font: Liberation Sans
- Heading 1: 24pt bold, light blue
- Heading 2: 18pt bold, lighter blue
- Heading 3: 14pt bold, lightest blue
- Body: 11pt, light gray
- Paragraph: 11pt with 1.5 line height

**Example:**
```csharp
var pdf = Pdf.Create()
    .WithTheme(PdfTheme.Dark)
    .AddPage(page => page
        .Body(b => b
            .Heading1("Modern Dark Document")
            .Text("Using the Dark theme")
        )
    )
    .Generate();
```

### Modern Theme

A minimalist theme with generous spacing and contemporary typography. Perfect for sleek, modern documents.

**Colors:**
- Primary: `#2C3E50` (dark slate)
- Text: `#2C3E50` (dark slate)
- Headings: Various shades of slate and gray
- Accents: `#E74C3C` (red)
- Muted: `#BDC3C7` (light gray)

**Typography:**
- Font: Liberation Sans
- Heading 1: 28pt bold (larger)
- Heading 2: 20pt bold
- Heading 3: 15pt bold
- Body: 11pt, dark slate
- Paragraph: 11pt with 1.7 line height (more spacious)

**Example:**
```csharp
var pdf = Pdf.Create()
    .WithTheme(PdfTheme.Modern)
    .AddPage(page => page
        .Body(b => b
            .Heading1("Contemporary Design")
            .Text("Using the Modern theme")
        )
    )
    .Generate();
```

## Theme Properties

Each theme defines:

### Text Styles
- **Text** - Regular body text
- **Heading 1** - Primary headings
- **Heading 2** - Secondary headings
- **Heading 3** - Tertiary headings
- **Paragraph** - Long-form text with line height
- **Accent** - Bold accent text for emphasis
- **Muted** - De-emphasized text
- **Caption** - Small caption text

### Component Themes
- **Table Theme** - Header colors, alternate row colors, borders
- **Line Theme** - Line color and stroke width
- **Header/Footer Theme** - Height and text styling

### Page Settings
- **Page Size** - Default A4
- **Orientation** - Portrait or landscape
- **Margins** - Top, bottom, left, right

## Applying Themes

### Use a Built-In Theme

```csharp
Pdf.Create()
    .WithTheme(PdfTheme.Default)  // or .Dark or .Modern
    .AddPage(/* ... */)
    .Generate();
```

### No Theme (Manual Styling)

If you don't apply a theme, elements use their specified styles or defaults:

```csharp
Pdf.Create()
    // No WithTheme() call
    .AddPage(page => page
        .Body(b => b
            .Text(t => t
                .FontSize(12)
                .Color("#000000")
            )
        )
    )
    .Generate();
```

## Custom Themes

### Create a Theme from Scratch

```csharp
var customTheme = new PdfTheme
{
    PageSettings = new PageSettings
    {
        PageSize = "A4",
        Orientation = "Portrait",
        Margins = new MarginSettings
        {
            Top = "25mm",
            Bottom = "25mm",
            Left = "20mm",
            Right = "20mm"
        }
    },
    Text = new PdfStyle
    {
        FontFamily = "Georgia",
        FontSize = 11,
        Color = "#1a1a1a"
    },
    Heading1 = new PdfStyle
    {
        FontFamily = "Georgia",
        FontSize = 24,
        FontWeight = "bold",
        Color = "#000099"
    },
    Heading2 = new PdfStyle
    {
        FontFamily = "Georgia",
        FontSize = 18,
        FontWeight = "bold",
        Color = "#333399"
    },
    Heading3 = new PdfStyle
    {
        FontFamily = "Georgia",
        FontSize = 14,
        FontWeight = "bold",
        Color = "#666699"
    },
    Paragraph = new PdfStyle
    {
        FontFamily = "Georgia",
        FontSize = 11,
        Color = "#1a1a1a",
        LineHeight = 1.6
    },
    Accent = new PdfStyle
    {
        FontFamily = "Georgia",
        FontSize = 11,
        FontWeight = "bold",
        Color = "#CC0000"
    },
    Muted = new PdfStyle
    {
        FontFamily = "Georgia",
        FontSize = 9,
        Color = "#999999"
    },
    Caption = new PdfStyle
    {
        FontFamily = "Georgia",
        FontSize = 9,
        Color = "#999999",
        Alignment = "center"
    },
    Table = new TableTheme
    {
        HeaderBackgroundColor = "#000099",
        HeaderTextColor = "#FFFFFF",
        HeaderFontWeight = "bold",
        AlternateRowColors = true,
        AlternateColor = "#F5F5F5",
        BorderColor = "#CCCCCC",
        BorderWidth = "0.5pt"
    },
    Line = new LineTheme
    {
        Color = "#CCCCCC",
        StrokeWidth = "1pt"
    }
};

Pdf.Create()
    .WithTheme(customTheme)
    .AddPage(/* ... */)
    .Generate();
```

### Extend an Existing Theme

Use `PdfThemeBuilder` to customize a base theme:

```csharp
var customTheme = new PdfThemeBuilder(PdfTheme.Default)
    .Heading1(h => h.Color = "#FF0000")  // Change heading 1 to red
    .Text(t => t.FontSize = 12)          // Increase base text size
    .Table(table =>
    {
        table.HeaderBackgroundColor = "#333333";
        table.AlternateColor = "#EEEEEE";
    })
    .Build();

Pdf.Create()
    .WithTheme(customTheme)
    .AddPage(/* ... */)
    .Generate();
```

## Style Hierarchy

When applying styles to elements, R4PDF follows this priority:

1. **Inline styles** (highest priority)
   ```csharp
   .Text(t => t.Color("#FF0000"))  // This wins
   ```

2. **Named styles** (custom or theme-based)
   ```csharp
   .Element(new TextElement { Style = "custom-heading" })
   ```

3. **Default theme styles** (lowest priority)
   ```csharp
   .Heading1("Title")  // Uses theme's heading1 style
   ```

**Example - Style Hierarchy in Action:**

```csharp
Pdf.Create()
    .WithTheme(PdfTheme.Default)  // Default heading1 is dark blue
    .AddStyle("section-title", new PdfStyle
    {
        FontSize = 16,
        Color = "#990000"  // Custom style overrides theme
    })
    .AddPage(page => page
        .Body(b => b
            .Heading1("Uses default theme (dark blue)")
            .Element(new TextElement
            {
                Text = "Uses custom section-title (dark red)",
                Style = "section-title"
            })
            .Text(t => t
                .Text("Uses inline override (green)")
                .Color("#00AA00")
            )
        )
    )
    .Generate();
```

## Font Families

R4PDF supports the following font families (via PdfSharpCore):

- `Liberation Sans` (default)
- `Liberation Serif`
- `Liberation Mono`
- `Courier New`
- `Times New Roman`
- `Arial`
- `Helvetica`
- `Verdana`
- `Georgia`
- `Comic Sans MS`

**Example:**
```csharp
var customTheme = new PdfTheme
{
    Text = new PdfStyle
    {
        FontFamily = "Georgia",
        FontSize = 12
    }
};
```

## Colors

R4PDF accepts colors in hex format:

```csharp
.Color("#FF0000")       // Red
.Color("#00FF00")       // Green
.Color("#0000FF")       // Blue
.Color("#FFFFFF")       // White
.Color("#000000")       // Black
.Color("#808080")       // Gray
```

## Theme Comparison

| Aspect | Default | Dark | Modern |
|--------|---------|------|--------|
| **Font** | Liberation Sans | Liberation Sans | Liberation Sans |
| **H1 Size** | 24pt | 24pt | 28pt |
| **H1 Color** | #003366 | #64B5F6 | #2C3E50 |
| **Body Color** | #333333 | #E0E0E0 | #2C3E50 |
| **Best For** | Business, Reports | Modern, Tech | Minimalist, Sleek |
| **Vibe** | Professional | Contemporary | Clean |

## Complete Theme Customization Example

```csharp
using R4PDF;
using R4PDF.Fluent;
using R4PDF.Models;

// Create a custom brand theme
var brandTheme = new PdfTheme
{
    PageSettings = new PageSettings
    {
        PageSize = "A4",
        Orientation = "Portrait",
        Margins = new MarginSettings { Top = "20mm", Bottom = "20mm", Left = "15mm", Right = "15mm" }
    },
    Text = new PdfStyle { FontFamily = "Arial", FontSize = 11, Color = "#333333" },
    Heading1 = new PdfStyle { FontFamily = "Arial", FontSize = 28, FontWeight = "bold", Color = "#0066CC" },
    Heading2 = new PdfStyle { FontFamily = "Arial", FontSize = 20, FontWeight = "bold", Color = "#3399FF" },
    Heading3 = new PdfStyle { FontFamily = "Arial", FontSize = 16, FontWeight = "bold", Color = "#66CCFF" },
    Paragraph = new PdfStyle { FontFamily = "Arial", FontSize = 11, Color = "#333333", LineHeight = 1.6 },
    Accent = new PdfStyle { FontFamily = "Arial", FontSize = 11, FontWeight = "bold", Color = "#FF6600" },
    Muted = new PdfStyle { FontFamily = "Arial", FontSize = 9, Color = "#999999" },
    Caption = new PdfStyle { FontFamily = "Arial", FontSize = 8, Color = "#CCCCCC", Alignment = "center" },
    Table = new TableTheme
    {
        HeaderBackgroundColor = "#0066CC",
        HeaderTextColor = "#FFFFFF",
        HeaderFontWeight = "bold",
        AlternateRowColors = true,
        AlternateColor = "#F0F5FF",
        BorderColor = "#DDDDDD",
        BorderWidth = "0.5pt"
    },
    Line = new LineTheme { Color = "#0066CC", StrokeWidth = "1pt" }
};

// Use the custom theme
var pdf = Pdf.Create()
    .WithTheme(brandTheme)
    .WithMetadata(meta => meta
        .Title("Company Report")
        .Author("Brand Team")
    )
    .AddPage(page => page
        .Body(b => b
            .Heading1("Our Custom Brand Theme")
            .Text("This document uses a custom theme with brand colors")
        )
    )
    .GenerateToFile("branded_report.pdf");
```

## See Also

- [Fluent API Guide](FLUENT_API.md) - Complete API reference
- [Code Examples](FLUENT_EXAMPLES.md) - More real-world examples
