using System.ComponentModel.DataAnnotations;

namespace RRMS.Application.Validation;

[AttributeUsage(
    AttributeTargets.Property
    | AttributeTargets.Field
    | AttributeTargets.Parameter,
    AllowMultiple = false)]
public sealed class DecimalPlacesAttribute : ValidationAttribute
{
    public DecimalPlacesAttribute(int maxDecimalPlaces)
    {
        if (maxDecimalPlaces < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxDecimalPlaces),
                "Decimal places must not be negative.");
        }

        MaxDecimalPlaces = maxDecimalPlaces;
        ErrorMessage = $"The {{0}} field must have at most {maxDecimalPlaces} decimal places.";
    }

    public int MaxDecimalPlaces { get; }

    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return true;
        }

        if (value is not decimal decimalValue)
        {
            return false;
        }

        return decimalValue == decimal.Round(
            decimalValue,
            MaxDecimalPlaces,
            MidpointRounding.ToZero);
    }
}
