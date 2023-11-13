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
        var differentDayPasses = passes.DistinctBy(x => x.Date).Count() > 1;
        if (differentDayPasses)
            throw new DomainException("Cant calculate toll fee for more than 1 day at a time");

        if (vehicle.IsTollFree)
            return 0;
        
        var sortedPasses = passes.OrderBy(x => x).ToList();
        return sortedPasses switch
        {
            [] => 0,
            [var first, ..] when IsTollFreeDate(first) => 0,
            _ => GetDailyTotalFee(sortedPasses)
        };
    }

    private int GetDailyTotalFee(IEnumerable<DateTime> sortedPasses)
        => Math.Min(_cityTollFee.MaxDailyFee, GroupByHourlyIntervals(sortedPasses.Select(x => x.TimeOfDay).ToList()).Sum(CalculateHourlyMax));

    private int CalculateHourlyMax(IEnumerable<TimeSpan> passes) =>
        passes.Select(_cityTollFee.GetCityTollFeeForDate).Max();

    private static IEnumerable<IGrouping<TimeSpan, TimeSpan>> GroupByHourlyIntervals(IReadOnlyList<TimeSpan> sortedPasses)
    {
        var oneHour = new TimeSpan(1, 0, 0);
        var slidingWindow = sortedPasses[0];
        return sortedPasses.GroupBy(x =>
        {
            if (x <= slidingWindow.Add(oneHour))
                return slidingWindow;
            
            slidingWindow = x;
            return x;
        }).ToList();
    }

    private bool IsTollFreeDate(DateTime date)
    {
        var dayAfter = date.AddDays(1);

        var isJuly = date.Month is 7;
        var isSunday = date.DayOfWeek is DayOfWeek.Sunday;
        var isRedDay = IsRedDay(date, _cityTollFee.Country);
        var dayAfterIsRedDay = IsRedDay(dayAfter, _cityTollFee.Country);

        return isJuly || isSunday || isRedDay || dayAfterIsRedDay;
    }

    private bool IsRedDay(DateTime date, Country country)
    {
        var holidays = _getPublicHolidays.GetPublicHolidays(date.Year, country);
        return date.DayOfWeek is DayOfWeek.Sunday || holidays.Contains(date);
    }
}