using PdfSharpCore.Fonts;

namespace R4PDF.Rendering;

/// <summary>
/// Resolves font families to system-installed TTF files.
/// Maps common font names (Helvetica, Arial, etc.) to Liberation Sans which is
/// metrically equivalent and available on most Linux distributions.
/// </summary>
public class SystemFontResolver : IFontResolver
{
    // Face name keys
    private const string Regular = "LiberationSans-Regular";
    private const string Bold = "LiberationSans-Bold";
    private const string Italic = "LiberationSans-Italic";
    private const string BoldItalic = "LiberationSans-BoldItalic";

    private static readonly Dictionary<string, string> FaceToFile = new(StringComparer.OrdinalIgnoreCase)
    {
        [Regular] = "LiberationSans-Regular.ttf",
        [Bold] = "LiberationSans-Bold.ttf",
        [Italic] = "LiberationSans-Italic.ttf",
        [BoldItalic] = "LiberationSans-BoldItalic.ttf",
    };

    private static readonly string[] SearchPaths =
    [
        "/usr/share/fonts/liberation",
        "/usr/share/fonts/truetype/liberation",
        "/usr/share/fonts/TTF",
        "/usr/share/fonts/truetype",
        "/usr/share/fonts",
        "/usr/local/share/fonts",
    ];

    // Font names that should be mapped to Liberation Sans
    private static readonly HashSet<string> SansSerifAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        FontFamilies.Helvetica, FontFamilies.Arial, FontFamilies.LiberationSans,
        FontFamilies.SansSerif, FontFamilies.Verdana, FontFamilies.Tahoma,
        FontFamilies.SegoeUI, FontFamilies.Calibri,
    };

    public string DefaultFontName => FontFamilies.LiberationSans;

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // Map all common sans-serif families and any unknown font to Liberation Sans
        var faceName = (isBold, isItalic) switch
        {
            (true, true) => BoldItalic,
            (true, false) => Bold,
            (false, true) => Italic,
            _ => Regular,
        };

        return new FontResolverInfo(faceName);
    }

    public byte[] GetFont(string faceName)
    {
        if (!FaceToFile.TryGetValue(faceName, out var fileName))
            fileName = FaceToFile[Regular];

        foreach (var dir in SearchPaths)
        {
            var path = Path.Combine(dir, fileName);
            if (File.Exists(path))
                return File.ReadAllBytes(path);
        }

        // Deep search as fallback
        foreach (var dir in SearchPaths.Where(Directory.Exists))
        {
            var found = Directory.GetFiles(dir, fileName, SearchOption.AllDirectories).FirstOrDefault();
            if (found != null)
                return File.ReadAllBytes(found);
        }

        throw new FileNotFoundException(
            $"Could not find font file '{fileName}'. Ensure liberation-fonts are installed (e.g. 'sudo pacman -S ttf-liberation' or 'sudo apt install fonts-liberation').");
    }

    /// <summary>
    /// Ensures the font resolver is registered. Safe to call multiple times.
    /// </summary>
    internal static void EnsureRegistered()
    {
        if (GlobalFontSettings.FontResolver is SystemFontResolver)
            return;

        GlobalFontSettings.FontResolver = new SystemFontResolver();
    }
}
