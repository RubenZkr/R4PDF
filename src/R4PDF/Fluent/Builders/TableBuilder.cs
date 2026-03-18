using R4PDF.Fluent.Options;
using R4PDF.Fluent.Themes;
using R4PDF.Models;
using R4PDF.Models.Elements;

namespace R4PDF.Fluent.Builders;

/// <summary>
/// Builder for table elements. Theme table defaults are applied automatically at build time.
/// </summary>
public class TableBuilder
{
    private readonly TableTheme? _tableTheme;
    private readonly List<TableColumn> _columns = new();
    private readonly List<TableRow> _rows = new();
    private PdfStyle? _headerStyle;
    private bool? _alternateRowColors;
    private string? _alternateColor;
    private BorderStyle? _borders;
    private bool _showHeader = true;

    internal TableBuilder(TableTheme? tableTheme)
    {
        _tableTheme = tableTheme;
    }

    public TableBuilder Column(string name, string? width = null, string? alignment = null)
    {
        _columns.Add(new TableColumn
        {
            Name = name,
            Width = width,
            Alignment = alignment
        });
        return this;
    }

    public TableBuilder Row(params string[] cells)
    {
        _rows.Add(new TableRow { Cells = cells.ToList() });
        return this;
    }

    public TableBuilder Row(string[] cells, string? backgroundColor = null, string? textColor = null)
    {
        _rows.Add(new TableRow
        {
            Cells = cells.ToList(),
            BackgroundColor = backgroundColor,
            TextColor = textColor
        });
        return this;
    }

    public TableBuilder HeaderStyle(Action<PdfStyle> configure)
    {
        _headerStyle ??= new PdfStyle();
        configure(_headerStyle);
        return this;
    }

    public TableBuilder AlternateRowColor(string color)
    {
        _alternateRowColors = true;
        _alternateColor = color;
        return this;
    }

    public TableBuilder Borders(string width, string color)
    {
        _borders = new BorderStyle { Width = width, Color = color };
        return this;
    }

    public TableBuilder ShowHeader(bool show)
    {
        _showHeader = show;
        return this;
    }

    internal TableElement Build()
    {
        var element = new TableElement
        {
            Columns = _columns,
            Rows = _rows,
            ShowHeader = _showHeader
        };

        // Apply theme defaults, then user overrides
        if (_tableTheme != null)
        {
            element.HeaderStyle = new PdfStyle
            {
                BackgroundColor = _tableTheme.HeaderBackgroundColor,
                Color = _tableTheme.HeaderTextColor,
                FontWeight = _tableTheme.HeaderFontWeight,
                FontSize = _tableTheme.HeaderFontSize
            };
            element.AlternateRowColors = _tableTheme.AlternateRowColors;
            element.AlternateColor = _tableTheme.AlternateColor;
            element.Borders = new BorderStyle
            {
                Width = _tableTheme.BorderWidth,
                Color = _tableTheme.BorderColor
            };
        }

        // User overrides take priority
        if (_headerStyle != null)
        {
            element.HeaderStyle ??= new PdfStyle();
            if (_headerStyle.BackgroundColor != null) element.HeaderStyle.BackgroundColor = _headerStyle.BackgroundColor;
            if (_headerStyle.Color != null) element.HeaderStyle.Color = _headerStyle.Color;
            if (_headerStyle.FontWeight != null) element.HeaderStyle.FontWeight = _headerStyle.FontWeight;
            if (_headerStyle.FontSize.HasValue) element.HeaderStyle.FontSize = _headerStyle.FontSize;
        }

        if (_alternateRowColors.HasValue) element.AlternateRowColors = _alternateRowColors.Value;
        if (_alternateColor != null) element.AlternateColor = _alternateColor;
        if (_borders != null) element.Borders = _borders;

        return element;
    }
}
