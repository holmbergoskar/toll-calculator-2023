namespace TollFeeCalculator;

public abstract record Vehicle(bool IsTollFree);
public record Motorbike() : Vehicle(true);
public record Car() : Vehicle(false);
