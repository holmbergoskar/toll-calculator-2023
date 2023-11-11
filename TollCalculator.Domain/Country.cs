namespace TollFeeCalculator;

public record Country(string CountryCode)
{
    public Country Sweden => new("SE");
}