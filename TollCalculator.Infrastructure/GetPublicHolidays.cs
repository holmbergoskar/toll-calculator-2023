using TollFeeCalculator;

namespace TollCalculator.Infrastructure;

public class GetPublicHolidaysQuery : IGetPublicHolidaysQuery
{
    public IEnumerable<DateTime> GetPublicHolidays(int year, Country country) => new List<DateTime>();

}