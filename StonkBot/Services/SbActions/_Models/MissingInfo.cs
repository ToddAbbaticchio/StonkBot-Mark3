using StonkBot.Extensions;

namespace StonkBot.Services.SbActions._Models;

public class MissingInfo
{
    public string Symbol { get; set; }
    public DateTime Date { get; set; }

    public MissingInfo(string symbol, DateTime date)
    {
        Symbol = symbol;
        Date = date.SbDate();
    }
}