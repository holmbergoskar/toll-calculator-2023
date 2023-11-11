namespace TollFeeCalculator;

public record TollInterval(TimeSpan Start, TimeSpan End, int Fee)
{
    public bool IsInRange(TimeSpan timeSpan) => timeSpan >= Start && timeSpan <= End;
}