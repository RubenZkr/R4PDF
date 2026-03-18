using PdfSharpCore.Drawing;
using R4PDF.Parsing;

namespace R4PDF.Tests;

public class ColorParserTests
{
    [Fact]
    public void Parse_HexColor6_ReturnsCorrectColor()
    {
        var color = ColorParser.Parse("#FF0000");
        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void Parse_HexColor3_ReturnsCorrectColor()
    {
        var color = ColorParser.Parse("#F00");
        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Theory]
    [InlineData("black")]
    [InlineData("Black")]
    [InlineData("BLACK")]
    public void Parse_NamedColor_ReturnsCorrectColor(string name)
    {
        var color = ColorParser.Parse(name);
        Assert.Equal(0, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void Parse_NullOrEmpty_ReturnsBlack()
    {
        var color = ColorParser.Parse(null);
        Assert.Equal(0, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void Parse_WithDefault_ReturnsDefaultForNull()
    {
        var color = ColorParser.Parse(null, XColors.Red);
        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void Parse_InvalidColor_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => ColorParser.Parse("notacolor"));
        Assert.Throws<FormatException>(() => ColorParser.Parse("#GGGGGG"));
    }
}