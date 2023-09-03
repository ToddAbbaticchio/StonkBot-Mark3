using StonkBot.Extensions;

namespace StonkBot.Services.SbActions._Models;

public class IpoSecondPassAlert
{
    public string Symbol { get; set; }
    public string CheckDayClose { get; set; }
    public string OpeningDayLow { get; set; }
    public string TodayClose { get; set; }
    public string OpeningDayOpen { get; set; }
    public DateTime FirstCheckDate { get; set; }
    public DateTime TargetDate { get; set; }
    public int DaysSatisfied { get; set; }

    public IpoSecondPassAlert(string symbol, decimal checkDayClose, decimal openingDayLow, decimal todayClose, decimal openingDayOpen, DateTime fDate, DateTime tDate, int daysSatisfied)
    {
        Symbol = symbol;
        CheckDayClose = checkDayClose.Clean();
        OpeningDayLow = openingDayLow.Clean();
        TodayClose = todayClose.Clean();
        OpeningDayOpen = openingDayOpen.Clean();
        FirstCheckDate = fDate.SbDate();
        TargetDate = tDate.SbDate();
        DaysSatisfied = daysSatisfied;
    }
}