namespace TollFeeCalculator;

public interface IGetPublicHolidaysQuery
{
    public IEnumerable<DateTime> GetPublicHolidays(int year, Country countryCode);
}