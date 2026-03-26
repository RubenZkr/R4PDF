using PdfSharpCore.Drawing;
using R4PDF.Models.Elements;
using R4PDF.Parsing;

namespace R4PDF.Rendering;

public class ImageRenderer
{
    public double Render(XGraphics gfx, ImageElement element, double x, double y, double availableWidth)
    {
        XImage image;

        if (element.Source.StartsWith("data:"))
        {
            // Base64 data URI: data:image/png;base64,iVBOR...
            var commaIndex = element.Source.IndexOf(',');
            if (commaIndex < 0)
                throw new FormatException("Invalid base64 data URI for image.");

            var base64 = element.Source[(commaIndex + 1)..].Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");
            var bytes = Convert.FromBase64String(base64);
            using var ms = new MemoryStream(bytes);
            image = XImage.FromStream(() => new MemoryStream(bytes));
        }
        else if (File.Exists(element.Source))
        {
            image = XImage.FromFile(element.Source);
        }
        else
        {
            throw new FileNotFoundException($"Image file not found: '{element.Source}'");
        }

        // Calculate dimensions
        var targetWidth = element.Width != null ? UnitConverter.ToPoints(element.Width) : 0;
        var targetHeight = element.Height != null ? UnitConverter.ToPoints(element.Height) : 0;

        if (targetWidth == 0 && targetHeight == 0)
        {
            targetWidth = Math.Min(image.PointWidth, availableWidth);
            targetHeight = image.PointHeight * (targetWidth / image.PointWidth);
        }
        else if (targetWidth > 0 && targetHeight == 0 && element.MaintainAspectRatio)
        {
            targetHeight = image.PointHeight * (targetWidth / image.PointWidth);
        }
        else if (targetHeight > 0 && targetWidth == 0 && element.MaintainAspectRatio)
        {
            targetWidth = image.PointWidth * (targetHeight / image.PointHeight);
        }

        // Handle alignment
        var drawX = x;
        if (element.Alignment?.Equals(Alignments.Center, StringComparison.OrdinalIgnoreCase) == true)
            drawX = x + (availableWidth - targetWidth) / 2;
        else if (element.Alignment?.Equals(Alignments.Right, StringComparison.OrdinalIgnoreCase) == true)
            drawX = x + availableWidth - targetWidth;

        gfx.DrawImage(image, drawX, y, targetWidth, targetHeight);

        return targetHeight;
    }
}