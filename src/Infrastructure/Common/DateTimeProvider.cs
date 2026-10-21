using Roomfolio.Application.Abstractions;

namespace Roomfolio.Infrastructure.Common;

public class DateTimeProvider : IDateTimeProvider
{
    private static readonly TimeZoneInfo BangkokTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateOnly TodayInBangkok
    {
        get
        {
            var bangkokNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, BangkokTimeZone);
            return DateOnly.FromDateTime(bangkokNow.DateTime);
        }
    }
}
