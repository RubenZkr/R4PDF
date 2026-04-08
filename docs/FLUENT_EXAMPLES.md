# R4PDF Fluent API Examples

Real-world examples demonstrating the fluent API for common PDF generation scenarios.

## Table of Contents

- [Invoice](#invoice)
- [Business Report](#business-report)
- [Letter](#letter)
- [Data Table Export](#data-table-export)
- [Certificate](#certificate)
- [Email-Friendly Document](#email-friendly-document)
- [Automatic Continuation Pages](#automatic-continuation-pages)

---

## Invoice

Generate professional invoices with itemized tables and formatting.

```csharp
using R4PDF;
using R4PDF.Fluent;

public byte[] GenerateInvoice(
    string invoiceNumber,
    DateTime invoiceDate,
    string companyName,
    string customerName,
    List<(string product, int qty, decimal price)> items,
    decimal taxRate = 0.21m)
{
    var subtotal = items.Sum(i => i.qty * i.price);
    var taxAmount = subtotal * taxRate;
    var total = subtotal + taxAmount;

    var pdf = Pdf.Create()
        .WithTheme(PdfTheme.Default)
        .WithMetadata(meta => meta
            .Title($"Invoice {invoiceNumber}")
            .Author(companyName)
        )
        .AddPage(page => page
            .Body(b => b
                // Header
                .Heading1(companyName)
                .Text("Company Details").MutedText()
                .Paragraph("123 Business Street\nCity, Country 12345\ninfo@company.com")
                .Spacer("15mm")

                // Invoice Info
                .Table(t => t
                    .Column("Invoice Number")
                    .Column("Date")
                    .Column("Due Date")
                    .Row(invoiceNumber, invoiceDate.ToString("MMM dd, yyyy"), invoiceDate.AddDays(30).ToString("MMM dd, yyyy"))
                )
                .Spacer("10mm")

                // Customer Info
                .Text("Bill To:").AccentText()
                .Paragraph(customerName)
                .Spacer("10mm")

                // Items Table
                .Text("Items").AccentText()
                .Table(t => t
                    .Column("Description")
                    .Column("Quantity")
                    .Column("Unit Price")
                    .Column("Total")
                    .Row(t =>
                    {
                        foreach (var item in items)
                        {
                            var itemTotal = item.qty * item.price;
                            t.Row(
                                item.product,
                                item.qty.ToString(),
                                $"${item.price:F2}",
                                $"${itemTotal:F2}"
                            );
                        }
                    })
                )
                .Spacer("10mm")

                // Totals
                .Line(l => l.Color("#CCCCCC"))
                .Spacer("5mm")
                .Text($"Subtotal: ${subtotal:F2}")
                .Text($"Tax ({(taxRate * 100):F1}%): ${taxAmount:F2}")
                .Text($"Total: ${total:F2}").AccentText()
                .Spacer("10mm")

                // Footer
                .Paragraph("Thank you for your business!")
                .CaptionText()
            )
        )
        .Generate();

    return pdf;
}

// Usage
var invoice = GenerateInvoice(
    "INV-2026-001",
    DateTime.Now,
    "Tech Solutions Inc.",
    "John Smith",
    new()
    {
        ("Consulting Services", 10, 150m),
        ("Software License", 5, 200m),
        ("Training Hours", 20, 75m)
    }
);

File.WriteAllBytes("invoice.pdf", invoice);
```

---

## Automatic Continuation Pages

Automatically continue long body content to the next page instead of manually chunking data.

```csharp
using R4PDF;
using R4PDF.Fluent;

var longParagraph = string.Join(" ", Enumerable.Repeat(
    "This paragraph is intentionally long so the renderer has to continue to a new page automatically.",
    180));

var bytes = Pdf.Create()
    .WithTheme(PdfTheme.Default)
    .WithAutoPagination(a => a
        .Enabled()
        .RepeatHeaderOnContinuation()
        .RepeatFooterOnContinuation()
        .SplitParagraphs()
        .SplitTables())
    .AddPage(page => page
        .Header(h => h.Text("Auto-pagination demo"))
        .Body(b => b
            .Heading1("Long document")
            .Paragraph(longParagraph)
            .Table(t => t
                .Column("#", "20%")
                .Column("Description", "80%")
                .Row("1", "Row one")
                .Row("2", "Row two")
                .Row("3", "Row three")
                .Row("4", "Row four")
                .Row("5", "Row five")
                .Row("6", "Row six")
                .Row("7", "Row seven")
                .Row("8", "Row eight")
                .Row("9", "Row nine")
                .Row("10", "Row ten")))
        .Footer(f => f.CaptionText("Page {pageNumber} of {pageCount}")))
    .Generate();

File.WriteAllBytes("auto-pagination.pdf", bytes);
```

---

## Business Report

Create a multi-section business report with charts represented as tables and statistics.

```csharp
using R4PDF;
using R4PDF.Fluent;

public byte[] GenerateQuarterlyReport(
    string quarter,
    int year,
    Dictionary<string, decimal> metrics,
    Dictionary<string, List<(string month, decimal value)>> monthlyData)
{
    return Pdf.Create()
        .WithTheme(PdfTheme.Modern)
        .WithMetadata(meta => meta
            .Title($"{quarter} {year} Quarterly Report")
            .Author("Management Team")
            .Subject("Financial Performance")
        )
        .AddPage(page => page
            .Settings(s => s
                .PageSize("A4")
                .Orientation("Portrait")
                .Margins("25mm")
            )
            .Body(b => b
                // Title
                .Heading1($"{quarter} {year} Quarterly Report")
                .Text($"Fiscal Year {year}")
                .MutedText()
                .Spacer("20mm")

                // Executive Summary
                .Heading2("Executive Summary")
                .Paragraph(
                    "This report provides a comprehensive overview of business performance during " +
                    $"{quarter} {year}, including key metrics, regional analysis, and strategic initiatives.")
                .Spacer("15mm")

                // Key Metrics
                .Heading2("Key Performance Indicators")
                .Table(t => t
                    .Column("Metric")
                    .Column("Value")
                    .Column("vs. Last Quarter")
                    .Row(metrics.Select(m =>
                        (m.Key, $"${m.Value:F2}", "+12%")
                    ).ToArray())
                )
                .Spacer("15mm")

                // Regional Analysis
                .Heading2("Regional Performance")
                .Paragraph(
                    "Performance remained strong across all regional markets with consistent " +
                    "growth in both new and existing customer segments.")
                .Spacer("10mm")
                .Table(t => t
                    .Column("Region")
                    .Column("Revenue")
                    .Column("Growth")
                    .Column("Market Share")
                    .Row("North America", "$2,450,000", "+15%", "42%")
                    .Row("Europe", "$1,820,000", "+9%", "31%")
                    .Row("Asia Pacific", "$980,000", "+22%", "17%")
                    .Row("Other", "$550,000", "+5%", "10%")
                )
                .Spacer("15mm")

                // Monthly Trend
                .Heading2("Monthly Trends")
                .Paragraph("Revenue trend across the quarter:")
                .Spacer("5mm")
                .Table(t => t
                    .Column("Month")
                    .Column("Value")
                    .Row(monthlyData.First().Value.Select((item, i) =>
                        (item.month, item.value.ToString("C"))
                    ).ToArray())
                )
                .Spacer("15mm")

                // Outlook
                .Heading2("Outlook & Strategic Initiatives")
                .Paragraph($@"
Q{(quarter == "Q1" ? 2 : int.Parse(quarter.Substring(1)) + 1)} {year} shows promise with:
• Expanded market penetration in emerging regions
• Launch of new product lines
• Enhanced customer engagement programs
• Continued investment in digital transformation
                ")
                .Spacer("15mm")

                // Footer
                .CaptionText("Confidential - For Internal Use Only")
            )
        )
        .Generate();
}

// Usage
var report = GenerateQuarterlyReport(
    "Q1",
    2026,
    new()
    {
        { "Total Revenue", 5800000m },
        { "Operating Income", 1450000m },
        { "Net Profit Margin", 0.25m }
    },
    new()
    {
        { "Revenue", new()
            {
                ("January", 1850000m),
                ("February", 1950000m),
                ("March", 2000000m)
            }
        }
    }
);

File.WriteAllBytes("q1_report.pdf", report);
```

---

## Letter

Professional business letter with proper formatting.

```csharp
using R4PDF;
using R4PDF.Fluent;

public byte[] GenerateLetter(
    string recipientName,
    string recipientCompany,
    string recipientAddress,
    string subject,
    string salutation,
    string letterBody,
    string senderName,
    string senderTitle,
    string senderCompany)
{
    return Pdf.Create()
        .WithTheme(PdfTheme.Default)
        .AddPage(page => page
            .Body(b => b
                // Sender Info
                .Paragraph($"{senderCompany}\n123 Corporate Drive\nCity, State 12345\nwww.company.com")
                .Spacer("20mm")

                // Date
                .Text(DateTime.Now.ToString("MMMM d, yyyy"))
                .Spacer("10mm")

                // Recipient
                .Paragraph($@"{recipientName}
{recipientCompany}
{recipientAddress}")
                .Spacer("10mm")

                // Subject
                .Text($"Subject: {subject}").AccentText()
                .Spacer("10mm")

                // Salutation
                .Text($"Dear {salutation},")
                .Spacer("5mm")

                // Body
                .Paragraph(letterBody)
                .Spacer("10mm")

                // Closing
                .Text("Sincerely,")
                .Spacer("20mm")

                // Signature Area
                .Text(senderName).AccentText()
                .Text(senderTitle)
            )
        )
        .Generate();
}

// Usage
var letter = GenerateLetter(
    "Jane Johnson",
    "Future Corp",
    "456 Tech Street, Innovation City, State 54321",
    "Partnership Proposal",
    "Ms. Johnson",
    @"I hope this letter finds you well. I am writing to propose a strategic partnership 
that would benefit both of our organizations. Over the past few months, we have identified 
significant opportunities for collaboration in the emerging technology sector.

Our teams have developed a comprehensive proposal that outlines potential synergies, 
shared resources, and projected outcomes. We believe this partnership would position 
both companies for accelerated growth and market expansion.

I would welcome the opportunity to discuss this proposal at your earliest convenience. 
Please let me know your availability for a meeting in the coming weeks.

Thank you for considering this partnership opportunity. I look forward to hearing from you.",
    "Michael Chen",
    "VP of Business Development",
    "TechVision Solutions"
);

File.WriteAllBytes("partnership_letter.pdf", letter);
```

---

## Data Table Export

Export data as a formatted, styled table in PDF.

```csharp
using R4PDF;
using R4PDF.Fluent;

public class DataRecord
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; }
}

public byte[] GenerateDataTable(List<DataRecord> records, string title)
{
    return Pdf.Create()
        .WithTheme(PdfTheme.Modern)
        .WithMetadata(meta => meta
            .Title(title)
            .Author("Data Export")
        )
        .AddPage(page => page
            .Body(b => b
                .Heading1(title)
                .Text($"Generated: {DateTime.Now:MMMM d, yyyy HH:mm}")
                .MutedText()
                .Spacer("10mm")

                .Paragraph($"Total Records: {records.Count}")
                .Spacer("10mm")

                .Table(t => t
                    .Column("ID")
                    .Column("Name")
                    .Column("Category")
                    .Column("Amount")
                    .Column("Date")
                    .Column("Status")
                    .Row(records.Select(r => (
                        r.Id,
                        r.Name,
                        r.Category,
                        $"${r.Amount:F2}",
                        r.Date.ToString("MMM dd, yyyy"),
                        r.Status
                    )).ToArray())
                    .Borders("0.5pt", "#CCCCCC")
                    .AlternateRowColor("#F9FAFB")
                )
                .Spacer("10mm")

                .CaptionText($"Dataset contains {records.Count} records | Export Date: {DateTime.Now:MMMM d, yyyy}")
            )
        )
        .Generate();
}

// Usage
var records = new List<DataRecord>
{
    new() { Id = "001", Name = "John Doe", Category = "Sales", Amount = 5000m, Date = DateTime.Now.AddDays(-5), Status = "Completed" },
    new() { Id = "002", Name = "Jane Smith", Category = "Marketing", Amount = 3500m, Date = DateTime.Now.AddDays(-3), Status = "Pending" },
    new() { Id = "003", Name = "Bob Wilson", Category = "Operations", Amount = 4200m, Date = DateTime.Now.AddDays(-1), Status = "Completed" },
};

var pdf = GenerateDataTable(records, "Q1 2026 Transaction Report");
File.WriteAllBytes("transactions.pdf", pdf);
```

---

## Certificate

Create a professional certificate or achievement document.

```csharp
using R4PDF;
using R4PDF.Fluent;

public byte[] GenerateCertificate(
    string recipientName,
    string achievementTitle,
    DateTime issueDate,
    string issuerName,
    string issuerTitle)
{
    return Pdf.Create()
        .WithTheme(PdfTheme.Default)
        .AddPage(page => page
            .Settings(s => s
                .PageSize("A4")
                .Orientation("Landscape")
            )
            .Body(b => b
                // Border effect with rectangle
                .Rectangle(r => r
                    .Width("95%")
                    .Height("90%")
                    .StrokeColor("#003366")
                    .StrokeWidth("3pt")
                    .FillColor("#FFFFFF")
                )
                .Spacer("20mm")

                // Certificate Title
                .Heading1("Certificate of Achievement")
                .Text("This is to certify that")
                .Spacer("10mm")

                // Recipient Name
                .Text(recipientName)
                .AccentText()
                .Spacer("10mm")

                // Achievement
                .Paragraph($"Has successfully completed and demonstrated proficiency in")
                .Spacer("5mm")
                .Text(achievementTitle)
                .AccentText()
                .Spacer("20mm")

                // Details
                .Text($"Issued on {issueDate:MMMM d, yyyy}")
                .Spacer("20mm")

                // Signature
                .Text(issuerName)
                .AccentText()
                .Text(issuerTitle)
                .MutedText()
            )
        )
        .Generate();
}

// Usage
var certificate = GenerateCertificate(
    "Sarah Johnson",
    "Advanced C# Programming",
    DateTime.Now,
    "Dr. Michael Chen",
    "Director of Training Programs"
);

File.WriteAllBytes("certificate.pdf", certificate);
```

---

## Email-Friendly Document

Create a document optimized for email delivery and digital sharing.

```csharp
using R4PDF;
using R4PDF.Fluent;

public byte[] GenerateEmailFriendlyNotice(
    string recipientEmail,
    string subject,
    string message,
    string actionButtonText,
    string actionButtonUrl)
{
    return Pdf.Create()
        .WithTheme(PdfTheme.Modern)
        .WithMetadata(meta => meta
            .Title(subject)
        )
        .AddPage(page => page
            .Settings(s => s
                .PageSize("A4")
                .Margins("10mm")
            )
            .Body(b => b
                // Header with branding color
                .Rectangle(r => r
                    .Width("100%")
                    .Height("40mm")
                    .FillColor("#2C3E50")
                )
                .Spacer("-40mm")
                .Heading1("Important Notice")
                .Text("Action Required").AccentText()
                .Spacer("25mm")

                // Main Content
                .Heading2(subject)
                .Spacer("10mm")

                .Paragraph(message)
                .Spacer("15mm")

                // Action Call
                .Text("Next Steps:")
                .AccentText()
                .Paragraph($"1. Review the information above\n2. Take the required action at your earliest convenience\n3. Contact support if you have questions")
                .Spacer("15mm")

                // Button-like element
                .Rectangle(r => r
                    .Width("100mm")
                    .Height("10mm")
                    .FillColor("#0066CC")
                )
                .Spacer("-10mm")
                .Text(actionButtonText)
                .Spacer("10mm")
                .Text($"Or visit: {actionButtonUrl}")
                .CaptionText()
                .Spacer("20mm")

                // Footer
                .Line(l => l.Color("#CCCCCC"))
                .Spacer("5mm")
                .Paragraph($"This notice was sent to: {recipientEmail}\nGenerated: {DateTime.Now:MMMM d, yyyy HH:mm}")
                .CaptionText()
            )
        )
        .Generate();
}

// Usage
var notice = GenerateEmailFriendlyNotice(
    "user@example.com",
    "Account Verification Required",
    @"Your account has been flagged for security verification. This is a standard procedure to ensure your account security and protect your data. Please verify your identity by clicking the button below within 24 hours.

If you did not request this action, please contact our security team immediately.",
    "Verify Now",
    "https://example.com/verify?token=abc123"
);

File.WriteAllBytes("account_notice.pdf", notice);
```

---

## Tips for Best Results

1. **Default Theme** - Use for professional/business documents
2. **Modern Theme** - Use for contemporary, minimalist designs
3. **Dark Theme** - Use for tech/modern aesthetic
4. **Custom Themes** - Create brand-specific themes for consistency
5. **Spacing** - Use `Spacer()` to control whitespace
6. **Tables** - Use `AlternateRowColor()` for readability
7. **Emphasis** - Use `AccentText()` for important information
8. **Line Height** - Paragraphs default to 1.5x for readability

## See Also

- [Fluent API Guide](FLUENT_API.md) - Complete API reference
- [Themes & Customization](FLUENT_THEMES.md) - Theme system details
