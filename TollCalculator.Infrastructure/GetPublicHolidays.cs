using TollFeeCalculator;

namespace TollCalculator.Infrastructure;

public class GetPublicHolidaysQuery : IGetPublicHolidaysQuery
{
    public IEnumerable<DateTime> Get(int year, Country country) => new List<DateTime>();

}