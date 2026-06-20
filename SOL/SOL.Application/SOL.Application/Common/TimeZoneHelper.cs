namespace Template.Application.Common;

/// <summary>
/// Helper class for converting between UTC and Damascus timezone
/// Damascus timezone is UTC+2 (EET - Eastern European Time) in winter
/// and UTC+3 (EEST - Eastern European Summer Time) in summer
/// </summary>
public static class TimeZoneHelper
{
    // Damascus timezone identifier
    private static readonly TimeZoneInfo DamascusTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Damascus");

    /// <summary>
    /// Converts a DateTimeOffset from Damascus timezone to UTC
    /// Used when receiving data from the user (requests)
    /// </summary>
    /// <param name="damascusTime">Time in Damascus timezone</param>
    /// <returns>Time in UTC</returns>
    public static DateTime ConvertDamascusToUtc(DateTimeOffset damascusTime)
    {
        // Convert to UTC by removing the offset and treating it as Damascus time
        var dateTime = DateTime.SpecifyKind(damascusTime.DateTime, DateTimeKind.Unspecified);
        var utcTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, DamascusTimeZone);
        return utcTime;
    }

    /// <summary>
    /// Converts a DateTimeOffset from UTC to Damascus timezone
    /// Used when sending data to the user (responses)
    /// </summary>
    /// <param name="utcTime">Time in UTC</param>
    /// <returns>Time in Damascus timezone</returns>
    public static DateTimeOffset ConvertUtcToDamascus(DateTimeOffset utcTime)
    {
        var damascusTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime.UtcDateTime, DamascusTimeZone);
        var offset = DamascusTimeZone.GetUtcOffset(damascusTime);
        return new DateTimeOffset(damascusTime, offset);
    }

    /// <summary>
    /// Converts a nullable DateTimeOffset from Damascus timezone to UTC
    /// </summary>
    public static DateTime? ConvertDamascusToUtc(DateTimeOffset? damascusTime)
    {
        return damascusTime.HasValue ? ConvertDamascusToUtc(damascusTime.Value) : null;
    }

    /// <summary>
    /// Converts a nullable DateTimeOffset from UTC to Damascus timezone
    /// </summary>
    public static DateTimeOffset? ConvertUtcToDamascus(DateTimeOffset? utcTime)
    {
        return utcTime.HasValue ? ConvertUtcToDamascus(utcTime.Value) : null;
    }

    /// <summary>
    /// Extension method to convert DateTimeOffset from Damascus to UTC
    /// </summary>
    public static DateTimeOffset ToDamascusUtc(this DateTimeOffset damascusTime)
    {
        return ConvertDamascusToUtc(damascusTime);
    }

    /// <summary>
    /// Extension method to convert DateTimeOffset from UTC to Damascus
    /// </summary>
    public static DateTimeOffset FromUtcToDamascus(this DateTimeOffset utcTime)
    {
        return ConvertUtcToDamascus(utcTime);
    }
    public static DateTime FromUtcToDamascus(this DateTime utcTime)
    {
        return ConvertUtcToDamascus(utcTime).DateTime;
    }
}

