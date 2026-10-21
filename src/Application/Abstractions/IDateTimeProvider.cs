namespace Roomfolio.Application.Abstractions;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
    DateOnly TodayInBangkok { get; }
}
