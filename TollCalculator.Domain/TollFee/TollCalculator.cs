namespace TollFeeCalculator;

public class TollCalculator
{
    private readonly ICityTollFee _cityTollFee;
    private readonly IGetPublicHolidaysQuery _getPublicHolidays;

    public TollCalculator(ICityTollFee cityTollFee, IGetPublicHolidaysQuery getPublicHolidays)
    {
        _cityTollFee = cityTollFee;
        _getPublicHolidays = getPublicHolidays;
    }

    public int GetTollFee(Vehicle vehicle, DateTime[] passes)
    {
        if (vehicle.IsTollFree)
            return 0;
        
        var dailyPasses = DailyTollPasses.CreateDailyPasses(passes.ToList());
        return dailyPasses switch
        {
            [] => 0,
            [_] when IsTollFreeDate(passes[0]) => 0,
            _ => GetDailyTotalFee(dailyPasses)
        };
    }

    private int GetDailyTotalFee(DailyTollPasses sortedPasses) =>
        Math.Min(_cityTollFee.MaxDailyFee, sortedPasses.GroupByHourlyIntervals().Sum(CalculateHourlyMax));

    private int CalculateHourlyMax(IEnumerable<TimeSpan> passes) =>
        passes.Select(_cityTollFee.GetCityTollFeeForDate).Max();


    private bool IsTollFreeDate(DateTime date)
    {
        var dayAfter = date.AddDays(1);

        var isJuly = date.Month is 7;
        var isRedDay = IsRedDay(date, _cityTollFee.Country);
        var dayAfterIsRedDay = IsRedDay(dayAfter, _cityTollFee.Country);

        return isJuly || isRedDay || dayAfterIsRedDay;
    }

    private bool IsRedDay(DateTime date, Country country)
    {
        var holidays = _getPublicHolidays.Get(date.Year, country);
        return date.DayOfWeek is DayOfWeek.Sunday || holidays.Contains(date);
    }
}