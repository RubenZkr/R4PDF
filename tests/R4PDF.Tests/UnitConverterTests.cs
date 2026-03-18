using R4PDF.Parsing;

namespace R4PDF.Tests;

public class UnitConverterTests
{
    [Theory]
    [InlineData("72pt", 72)]
    [InlineData("1in", 72)]
    [InlineData("25.4mm", 72)]
    [InlineData("2.54cm", 72)]
    [InlineData("0pt", 0)]
    [InlineData("10mm", 28.3465)]
    [InlineData("96px", 72)]
    public void ToPoints_ValidMeasurements_ReturnsCorrectPoints(string input, double expected)
    {
        var result = UnitConverter.ToPoints(input);
        Assert.Equal(expected, result, 2);
    }

    [Fact]
    public void ToPoints_NullOrEmpty_ReturnsZero()
    {
        Assert.Equal(0, UnitConverter.ToPoints(null));
        Assert.Equal(0, UnitConverter.ToPoints(""));
        Assert.Equal(0, UnitConverter.ToPoints("   "));
    }

    [Fact]
    public void ToPoints_PlainNumber_ReturnsParsedValue()
    {
        Assert.Equal(42, UnitConverter.ToPoints("42"));
        Assert.Equal(12.5, UnitConverter.ToPoints("12.5"));
    }

    [Fact]
    public void ToPoints_InvalidFormat_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => UnitConverter.ToPoints("abc"));
        Assert.Throws<FormatException>(() => UnitConverter.ToPoints("20xx"));
    }

    [Fact]
    public void ToPoints_WithDefault_ReturnsDefaultForNull()
    {
        Assert.Equal(10, UnitConverter.ToPoints(null, 10));
        Assert.Equal(10, UnitConverter.ToPoints("", 10));
        Assert.Equal(72, UnitConverter.ToPoints("1in", 10));
    }
}