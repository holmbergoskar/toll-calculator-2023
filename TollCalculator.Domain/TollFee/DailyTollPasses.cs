namespace TollFeeCalculator;

public class DailyTollPasses : List<TimeSpan>
{
    public DailyTollPasses(IEnumerable<TimeSpan> allPasses)
    {
        AddRange(allPasses.Order());
    }

    public static DailyTollPasses CreateDailyPasses(List<DateTime> allPasses)
    {
        var differentDayPasses = allPasses.DistinctBy(x => x.Date).Count() > 1;
        if (differentDayPasses)
            throw new DomainException("Cant calculate toll fee for more than 1 day at a time");

        return new DailyTollPasses(allPasses.Select(x => x.TimeOfDay));
    }
    
    public IEnumerable<IGrouping<TimeSpan, TimeSpan>> GroupByHourlyIntervals()
    {
        var oneHour = new TimeSpan(1, 0, 0);
        var slidingWindow = this[0];
        return this.GroupBy(x =>
        {
            if (x <= slidingWindow.Add(oneHour))
                return slidingWindow;
            
            slidingWindow = x;
            return x;
        }).ToList();
    }
}