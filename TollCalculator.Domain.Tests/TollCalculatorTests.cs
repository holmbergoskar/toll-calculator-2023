using Nager.Holiday;
using TollCalculator.Infrastructure;
using TollFeeCalculator;

namespace Domain.Tests;

public class TollCalculatorSpecTest
{
    private static readonly IEnumerable<DateOnly> Holidays = new[]
    {
        new DateOnly(2013, 01, 01),
        new DateOnly(2013, 01, 06),
        new DateOnly(2013, 03, 29),
        new DateOnly(2013, 03, 31),
        new DateOnly(2013, 04, 01),
        new DateOnly(2013, 05, 01),
        new DateOnly(2013, 05, 09),
        new DateOnly(2013, 05, 19),
        new DateOnly(2013, 06, 06),
        new DateOnly(2013, 06, 22),
        new DateOnly(2013, 11, 02),
        new DateOnly(2013, 12, 25),
        new DateOnly(2013, 12, 26),
    };

    public static IEnumerable<object[]> HolidaysArgs = Holidays.Select(h => new object[] { h });

    private readonly TollFeeCalculator.TollCalculator _calculator = new(new GothenburgCityTollFee(), new GetPublicHolidaysQuery());

    [Fact]
    public void SaturdaysAreTollFree()
    {
        // Arrange
        var dates = new[]
        {
            DateTime.Parse("2013-01-19T06:00:00"),
            DateTime.Parse("2013-01-19T07:15:00")
        };
        var car = CreateCar();
        
        // Act
        var toll = _calculator.GetTollFee(car, dates);
        
        // Assert
        Assert.Equal(0, toll);
    }

    [Fact]
    public void SundaysAreTollFree()
    {
        // Arrange
        var dates = new[]
        {
            DateTime.Parse("2013-01-20T06:00:00"),
            DateTime.Parse("2013-01-20T07:15:00")
        };
        var car = CreateCar();
        
        // Act
        var toll = _calculator.GetTollFee(car, dates);
        
        // Assert
        Assert.Equal(0, toll);
    }

    [Fact]
    public void FeesShouldBeAddedIfTimeIsMoreThan60MinutesApart()
    {
        // Arrange
        var dates = new[]
        {
            DateTime.Parse("2013-01-03T06:00:00"),
            DateTime.Parse("2013-01-03T07:15:00")
        };
        var car = CreateCar();
        
        // Act
        var toll = _calculator.GetTollFee(car, dates);
        
        // Assert
        Assert.Equal(26, toll);
    }

    [Theory]
    [InlineData("2013-01-03T06:00:00", 8)]
    [InlineData("2013-01-03T07:15:00", 18)]
    [InlineData("2013-01-03T07:59:59", 18)]
    [InlineData("2013-01-03T14:45:00", 8)]
    [InlineData("2013-01-03T15:31:00", 18)]
    [InlineData("2013-01-03T18:35:00", 0)]
    public void HighestFeeSelectedAtDifferentHours(string dateTime, int cost)
    {
        // Arrange
        var dt = DateTime.Parse(dateTime);
        var car = new Car();
        
        // Act
        var toll = _calculator.GetTollFee(car, new[] { dt });
        
        // Assert
        Assert.Equal(cost, toll);
    }

    [Theory]
    [InlineData("2013-07-01T15:31:00")]
    [InlineData("2013-07-31T18:35:00")]
    public void JulyShouldBeTollFree(string dateTime)
    {
        // Arrange
        var car = new Car();
        var dt = DateTime.Parse(dateTime);
        
        // Act
        var toll = _calculator.GetTollFee(car, new []{ dt });
        
        // Assert
        Assert.Equal(0, toll);
    }

    [Theory]
    [MemberData(nameof(HolidaysArgs))]
    public void DaysBeforeHolidaysAreTollFree(DateOnly date)
    {
        var dt = date
            .AddDays(-1)
            .ToDateTime(new TimeOnly(15, 59, 23));
        var car = CreateCar();
        var toll = _calculator.GetTollFee(car, new[] { dt });
        Assert.Equal(0, toll);
    }

    [Theory]
    [MemberData(nameof(HolidaysArgs))]
    public void HolidaysAreTollFree(DateOnly date)
    {
        var dt = date.ToDateTime(new TimeOnly(15, 59, 23));
        var car = CreateCar();
        var toll = _calculator.GetTollFee(car, new[] { dt });
        Assert.Equal(0, toll);
    }

    [Theory]
    [InlineData("05:00:00", 0)]
    [InlineData("06:00:00", 8)]
    [InlineData("06:23:15", 8)]
    [InlineData("06:29:59", 8)]
    [InlineData("07:00:00", 18)]
    [InlineData("07:15:22", 18)]
    [InlineData("07:59:59", 18)]
    [InlineData("08:00:00", 13)]
    [InlineData("08:10:20", 13)]
    [InlineData("08:29:59", 13)]
    [InlineData("08:30:00", 8)]
    [InlineData("11:30:00", 8)]
    [InlineData("14:59:59", 8)]
    [InlineData("15:00:00", 13)]
    [InlineData("15:13:59", 13)]
    [InlineData("15:29:59", 13)]
    [InlineData("15:30:00", 18)]
    [InlineData("15:32:10", 18)]
    [InlineData("16:59:59", 18)]
    [InlineData("17:00:00", 13)]
    [InlineData("17:13:56", 13)]
    [InlineData("18:00:00", 8)]
    [InlineData("18:15:05", 8)]
    [InlineData("18:29:59", 8)]
    [InlineData("18:30:00", 0)]
    [InlineData("21:30:00", 0)]
    public void TollFeeDiffersThroughoutDay(string time, int expectedToll)
    {
        var date = new DateOnly(2013, 2, 13);
        var timeSpan = TimeOnly.Parse(time);
        var car = CreateCar();
        var toll = _calculator.GetTollFee(car, new[] { date.ToDateTime(timeSpan) });
        Assert.Equal(expectedToll, toll);
    }

    [Fact]
    public void ShouldPickHighestAtDifferentHours()
    {
        var dates = new[]
        {
            new DateTime(2013, 2, 13, 6, 0, 0),
            new DateTime(2013, 2, 13, 6, 59, 59),
            new DateTime(2013, 2, 13, 15, 0, 0),
            new DateTime(2013, 2, 13, 15, 59, 59),
        };
        var car = CreateCar();
        var toll = _calculator.GetTollFee(car, dates);
        Assert.Equal(31, toll);
    }

    [Fact]
    public void ShouldGroupCorrectlyWithinDifferentHours()
    {
        var dates = new[]
        {
            new DateTime(2013, 2, 13, 6, 0, 0), // 8
            new DateTime(2013, 2, 13, 6, 59, 59), // 13
            new DateTime(2013, 2, 13, 15, 28, 0), // 13
            new DateTime(2013, 2, 13, 16, 27, 59), // 18
        };
        var car = CreateCar();

        var toll = _calculator.GetTollFee(car, dates);
        Assert.Equal(31, toll);
    }

    private static Vehicle CreateCar()
    {
        return new Car();
    }
}