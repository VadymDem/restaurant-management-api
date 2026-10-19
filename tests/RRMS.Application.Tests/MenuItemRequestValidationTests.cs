using System.ComponentModel.DataAnnotations;
using RRMS.Application.DTOs.Menu;

namespace RRMS.Application.Tests;

public class MenuItemRequestValidationTests
{
    [Fact]
    public void ValidRequest_HasNoValidationErrors()
    {
        var request = new MenuItemRequest { ItemName = "Caesar Salad", Description = "Fresh", Price = 9.99m };

        Assert.Empty(Validate(request));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyName_IsInvalid(string name)
    {
        var request = new MenuItemRequest { ItemName = name, Description = null, Price = 9.99m };

        Assert.Contains(Validate(request), r => r.MemberNames.Contains("ItemName"));
    }

    [Fact]
    public void NameLongerThan150_IsInvalid()
    {
        var request = new MenuItemRequest
        {
            ItemName = new string('a', 151),
            Description = null,
            Price = 9.99m
        };

        Assert.Contains(Validate(request), r => r.MemberNames.Contains("ItemName"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(1000000.01)]
    public void PriceOutOfRange_IsInvalid(decimal price)
    {
        var request = new MenuItemRequest { ItemName = "Item", Description = null, Price = price };

        Assert.Contains(Validate(request), r => r.MemberNames.Contains("Price"));
    }

    [Theory]
    [InlineData(100.999)]
    [InlineData(0.001)]
    public void PriceWithMoreThanTwoDecimals_IsInvalid(decimal price)
    {
        var request = new MenuItemRequest { ItemName = "Item", Description = null, Price = price };

        Assert.Contains(Validate(request), r => r.MemberNames.Contains("Price"));
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(9.99)]
    [InlineData(1000000)]
    public void PriceWithAtMostTwoDecimals_IsValid(decimal price)
    {
        var request = new MenuItemRequest { ItemName = "Item", Description = null, Price = price };

        Assert.Empty(Validate(request));
    }

    private static List<ValidationResult> Validate(MenuItemRequest request)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            results,
            validateAllProperties: true);

        return results;
    }
}
