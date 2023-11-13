namespace TollFeeCalculator;

public interface IGetPublicHolidaysQuery
{
    public IEnumerable<DateTime> Get(int year, Country countryCode);
}