using System.Globalization;
using RRMS.Application.Validation;

namespace RRMS.Application.Tests;

public class DecimalPlacesAttributeTests
{
    [Theory]
    [InlineData("9.99", true)]
    [InlineData("0.01", true)]
    [InlineData("19.99", true)]
    [InlineData("1.10", true)]
    [InlineData("1000000", true)]
    [InlineData("100.999", false)]
    [InlineData("0.001", false)]
    [InlineData("9.9900001", false)]
    public void IsValid_ReflectsScale(string raw, bool expected)
    {
        var value = decimal.Parse(raw, CultureInfo.InvariantCulture);
        var attribute = new DecimalPlacesAttribute(2);

        Assert.Equal(expected, attribute.IsValid(value));
    }

    [Fact]
    public void IsValid_Null_IsValid()
    {
        var attribute = new DecimalPlacesAttribute(2);

        Assert.True(attribute.IsValid(null));
    }

    [Fact]
    public void IsValid_NonDecimal_IsInvalid()
    {
        var attribute = new DecimalPlacesAttribute(2);

        Assert.False(attribute.IsValid(1.25d));
    }

    [Fact]
    public void Constructor_NegativePlaces_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DecimalPlacesAttribute(-1));
    }
}
