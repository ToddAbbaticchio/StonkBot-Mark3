using StonkBot.Extensions.Enums;
using StonkBot.Options;

namespace StonkBot.Extensions;

public static class MarketExtensions
{
    public static MarketStatus GetMarketStatus(this DateTime date)
    {
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            return MarketStatus.ClosedWeekend;

        if (MarketHolidays.Dates.Contains(date.Date))
            return MarketStatus.ClosedHoliday;

        return MarketStatus.MarketOpen;
    }
    public static MarketStatus GetMarketStatus(this long date)
    {
        var dtDate = DateTimeOffset.FromUnixTimeMilliseconds(date).Date;
        if (dtDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            return MarketStatus.ClosedWeekend;

        if (MarketHolidays.Dates.Contains(dtDate.Date))
            return MarketStatus.ClosedHoliday;

        return MarketStatus.MarketOpen;
    }
    public static List<DateTime> GetPrevTradeDays(this DateTime startDate, int count)
    {
        var prevDaysList = new List<DateTime>();
        var checkDate = startDate;
        while (prevDaysList.Count < count)
        {
            while (startDate == checkDate || checkDate.GetMarketStatus() != MarketStatus.MarketOpen)
            {
                checkDate = checkDate.AddDays(-1);
            }

            prevDaysList.Add(checkDate.Date);
            startDate = checkDate;
        }

        return prevDaysList;
    }

    public static DateTime GetFollowingTradeDay(this DateTime startDate)
    {
        var nextDate = startDate;
        while (nextDate == startDate || nextDate.GetMarketStatus() != MarketStatus.MarketOpen)
        {
            nextDate = nextDate.AddDays(1);
        }

        return nextDate;
    }
    public static DateTime GetPreviousTradeDay(this DateTime startDate)
    {
        var prevDate = startDate;
        while (prevDate == startDate || prevDate.GetMarketStatus() != MarketStatus.MarketOpen)
        {
            prevDate = prevDate.AddDays(-1);
        }

        return prevDate;
    }

    public static int ToCandleIndex(this DateTime time, int minutes)
    {
        var est = time.ToEST();
        var start = time.RegularMarketStartTime();
        var totalMin = est - start;
        var index = (int)totalMin.TotalMinutes / minutes;
        return index;
    }

    public static DateTime RegularMarketStartTime(this DateTime time)
    {
        return time.ToEST().Date.AddHours(9).AddMinutes(30);
    }
    public static DateTime RegularMarketEndTime(this DateTime time)
    {
        return time.ToEST().Date.AddHours(16);
    }
    public static DateTime ExtendedMarketStartTime(this DateTime time)
    {
        return time.ToEST().Date.AddHours(7);
    }
    public static DateTime ExtendedMarketEndTime(this DateTime time)
    {
        return time.ToEST().Date.AddHours(20);
    }
}