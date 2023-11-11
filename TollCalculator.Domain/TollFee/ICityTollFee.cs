
namespace TollFeeCalculator;

public interface ICityTollFee
{
    public int GetCityTollFeeForDate(TimeSpan timeOfDay);
    public Country Country { get; }
    public int MaxDailyFee { get; }
}

public class GothenburgCityTollFee : ICityTollFee
{
    private readonly List<TollInterval> _tollFeeTimes = new()
    {
        new(HourMinute(6, 0), HourMinute(6, 30), 8),
        new(HourMinute(6, 30), HourMinute(7, 0), 13),
        new(HourMinute(7, 0), HourMinute(8, 0), 18),
        new(HourMinute(8, 0), HourMinute(8, 30), 13),
        new(HourMinute(15, 0), HourMinute(15, 30), 13),
        new(HourMinute(15, 30), HourMinute(17, 0), 18),
        new(HourMinute(17, 0), HourMinute(18, 0), 13),
        new(HourMinute(18, 0), HourMinute(18, 30), 8),
    };

    private static TimeSpan HourMinute(int hour, int minute) => new(hour, minute, 0);

    public int GetCityTollFeeForDate(TimeSpan timeOfDay)
        => _tollFeeTimes.FirstOrDefault(x => x.IsInRange(timeOfDay))?.Fee ?? 0;

    public Country Country => Country.Sweden;
    public int MaxDailyFee => 60;
}