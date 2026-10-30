namespace RRMS.Application.Options;

public sealed class RestaurantOptions
{
    public const string SectionName = "Restaurant";

    /// <summary>
    /// Time zone used to interpret reservation date and time.
    /// IANA identifiers (for example <c>Europe/Kyiv</c>) work on every platform
    /// supported by .NET 10.
    /// </summary>
    public string TimeZone { get; set; } = "Europe/Kyiv";
}
