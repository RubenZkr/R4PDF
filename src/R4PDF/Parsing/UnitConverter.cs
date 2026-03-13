using System.Globalization;
using System.Text.RegularExpressions;

namespace R4PDF.Parsing;

/// <summary>
/// Converts measurement strings like "20mm", "1in", "72pt", "2cm" to PDF points (1pt = 1/72 inch).
/// </summary>
public static partial class UnitConverter
{
    private const double PointsPerInch = 72.0;
    private const double PointsPerMm = PointsPerInch / 25.4;
    private const double PointsPerCm = PointsPerInch / 2.54;

    [GeneratedRegex(@"^(-?\d+(?:\.\d+)?)\s*(pt|mm|cm|in|px)$", RegexOptions.IgnoreCase)]
    private static partial Regex MeasurementRegex();

    public static double ToPoints(string? measurement)
    {
        if (string.IsNullOrWhiteSpace(measurement))
            return 0;

        var match = MeasurementRegex().Match(measurement.Trim());
        if (!match.Success)
        {
            if (double.TryParse(measurement.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var raw))
                return raw;

            throw new FormatException($"Invalid measurement format: '{measurement}'. Expected format like '20mm', '1in', '72pt', '2cm'.");
        }

        var value = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var unit = match.Groups[2].Value.ToLowerInvariant();

        return unit switch
        {
            "pt" => value,
            "mm" => value * PointsPerMm,
            "cm" => value * PointsPerCm,
            "in" => value * PointsPerInch,
            "px" => value * 0.75, // 1px = 0.75pt at 96dpi
            _ => throw new FormatException($"Unknown unit: '{unit}'")
        };
    }

    public static double ToPoints(string? measurement, double defaultValue)
    {
        if (string.IsNullOrWhiteSpace(measurement))
            return defaultValue;

        return ToPoints(measurement);
    }
}
