using R4PDF.Parsing;
using PdfSharpCore.Drawing;

namespace R4PDF.Tests;

public class ColorParserTests
{
    [Fact]
    public void Parse_HexColor6_ReturnsCorrectColor()
    {
        var color = ColorParser.Parse("#FF0000");
        Assert.Equal(255, (int)color.R);
        Assert.Equal(0, (int)color.G);
        Assert.Equal(0, (int)color.B);
    }

    [Fact]
    public void Parse_HexColor3_ReturnsCorrectColor()
    {
        var color = ColorParser.Parse("#F00");
        Assert.Equal(255, (int)color.R);
        Assert.Equal(0, (int)color.G);
        Assert.Equal(0, (int)color.B);
    }

    [Theory]
    [InlineData("black")]
    [InlineData("Black")]
    [InlineData("BLACK")]
    public void Parse_NamedColor_ReturnsCorrectColor(string name)
    {
        var color = ColorParser.Parse(name);
        Assert.Equal(0, (int)color.R);
        Assert.Equal(0, (int)color.G);
        Assert.Equal(0, (int)color.B);
    }

    [Fact]
    public void Parse_NullOrEmpty_ReturnsBlack()
    {
        var color = ColorParser.Parse(null);
        Assert.Equal(0, (int)color.R);
        Assert.Equal(0, (int)color.G);
        Assert.Equal(0, (int)color.B);
    }

    [Fact]
    public void Parse_WithDefault_ReturnsDefaultForNull()
    {
        var color = ColorParser.Parse(null, XColors.Red);
        Assert.Equal(255, (int)color.R);
        Assert.Equal(0, (int)color.G);
        Assert.Equal(0, (int)color.B);
    }

    [Fact]
    public void Parse_InvalidColor_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => ColorParser.Parse("notacolor"));
        Assert.Throws<FormatException>(() => ColorParser.Parse("#GGGGGG"));
    }
}
