namespace StonkBot.Extensions;

public static class DateTimeExtensions
{
    public static DateTime SbDate(this DateTime date)
    {
        return date.ToLocalTime().Date;
    }
    public static DateTime SbDate(this long date)
    {
        var dtDate = DateTimeOffset.FromUnixTimeMilliseconds(date).LocalDateTime.Date;
        return dtDate;
    }
    public static DateTime SbDate(this double date)
    {
        var dtDate = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(date)).LocalDateTime.Date;
        return dtDate;
    }

    public static DateTime SbDateTime(this double dateTime)
    {
        var dtDate = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(dateTime)).LocalDateTime;
        return dtDate;
    }
    public static DateTime SbDateTime(this long dateTime)
    {
        var dtDate = DateTimeOffset.FromUnixTimeMilliseconds(dateTime).LocalDateTime;
        return dtDate;
    }

    public static string SbDateString(this DateTime date)
    {
        //return date.ToLocalTime().Date.ToString("MM/dd/yyyy");
        return date.ToString("MM/dd/yyyy");
    }
    
    public static string SbDateString(this long date)
    {
        var dtDate = DateTimeOffset.FromUnixTimeMilliseconds(date).LocalDateTime.Date;
        return dtDate.ToString("MM/dd/yyyy");
    }

    public static double ToUnixTimeStamp(this string tokenTimestamp)
    {
        var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        var diff = Convert.ToDateTime(tokenTimestamp).ToUniversalTime() - origin;
        return Math.Floor(diff.TotalMilliseconds);
    }
    public static double ToUnixTimeStamp(this DateTime tokenTimestamp)
    {
        var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        var diff = tokenTimestamp.ToUniversalTime() - origin;
        return Math.Floor(diff.TotalMilliseconds);
    }

    public static DateTime ToEST(this DateTime time)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(time.ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
    }

    public static DateTime ToTheMinute(this DateTime time)
    {
        return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);
    }
}