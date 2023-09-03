using StonkBot.Extensions;

namespace StonkBot.Services.SbActions._Models;

public class IpoFirstPassAlert
{
    public string Symbol { get; set; }
    public string TodayClose { get; set; }
    public string OpeningHigh { get; set; }
    public int DaysSatisfied { get; set; }
    public DateTime TodayDate { get; set; }

    public IpoFirstPassAlert(string symbol, decimal todayClose, decimal? openingHigh, DateTime todayDate, int daysSatisfied)
    {
        Symbol = symbol;
        TodayClose = todayClose.Clean();
        OpeningHigh = openingHigh.Clean();
        DaysSatisfied = daysSatisfied;
        TodayDate = todayDate.SbDate();
    }
}