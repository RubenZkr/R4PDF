namespace R4PDF.Fluent.Themes;

/// <summary>
///     Theme settings for table elements.
/// </summary>
public class TableTheme
{
    public string HeaderBackgroundColor { get; set; } = "#003366";
    public string HeaderTextColor { get; set; } = "#FFFFFF";
    public string HeaderFontWeight { get; set; } = FontWeights.Bold;
    public double HeaderFontSize { get; set; } = PdfDefaults.TableHeaderFontSize;
    public double DataFontSize { get; set; } = PdfDefaults.TableDataFontSize;
    public bool AlternateRowColors { get; set; } = true;
    public string AlternateColor { get; set; } = "#F8F9FA";
    public string BorderColor { get; set; } = "#DEE2E6";
    public string BorderWidth { get; set; } = "0.5pt";

    public TableTheme Clone()
    {
        return (TableTheme)MemberwiseClone();
    }
}