using System.Globalization;
using PdfSharpCore.Drawing;

namespace R4PDF.Parsing;

/// <summary>
///     Parses color strings (#RRGGBB, #RGB, named colors) into XColor values.
/// </summary>
public static class ColorParser
{
    private static readonly Dictionary<string, XColor> NamedColors = new(StringComparer.OrdinalIgnoreCase)
    {
        [Colors.Black] = XColors.Black,
        [Colors.White] = XColors.White,
        [Colors.Red] = XColors.Red,
        [Colors.Green] = XColors.Green,
        [Colors.Blue] = XColors.Blue,
        [Colors.Gray] = XColors.Gray,
        [Colors.Grey] = XColors.Gray,
        [Colors.Yellow] = XColors.Yellow,
        [Colors.Orange] = XColors.Orange,
        [Colors.Purple] = XColors.Purple,
        [Colors.DarkGray] = XColors.DarkGray,
        [Colors.DarkGrey] = XColors.DarkGray,
        [Colors.LightGray] = XColors.LightGray,
        [Colors.LightGrey] = XColors.LightGray,
        [Colors.Transparent] = XColor.FromArgb(0, 0, 0, 0)
    };

    public static XColor Parse(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return XColors.Black;

        color = color.Trim();

        if (NamedColors.TryGetValue(color, out var named))
            return named;

        if (color.StartsWith('#'))
        {
            var hex = color[1..];
            return hex.Length switch
            {
                3 => XColor.FromArgb(
                    int.Parse(new string(hex[0], 2), NumberStyles.HexNumber),
                    int.Parse(new string(hex[1], 2), NumberStyles.HexNumber),
                    int.Parse(new string(hex[2], 2), NumberStyles.HexNumber)),
                6 => XColor.FromArgb(
                    int.Parse(hex[..2], NumberStyles.HexNumber),
                    int.Parse(hex[2..4], NumberStyles.HexNumber),
                    int.Parse(hex[4..6], NumberStyles.HexNumber)),
                8 => XColor.FromArgb(
                    int.Parse(hex[..2], NumberStyles.HexNumber),
                    int.Parse(hex[2..4], NumberStyles.HexNumber),
                    int.Parse(hex[4..6], NumberStyles.HexNumber),
                    int.Parse(hex[6..8], NumberStyles.HexNumber)),
                _ => throw new FormatException($"Invalid hex color: '{color}'")
            };
        }

        throw new FormatException($"Unknown color format: '{color}'");
    }

    public static XColor Parse(string? color, XColor defaultColor)
    {
        if (string.IsNullOrWhiteSpace(color))
            return defaultColor;

        return Parse(color);
    }
}