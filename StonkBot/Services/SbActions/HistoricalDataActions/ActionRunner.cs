namespace StonkBot.Services.SbActions;

public partial interface ISbAction
{
    Task GetMarketData(CancellationToken cToken);
}

internal partial class SbAction
{
    public async Task GetMarketData(CancellationToken cToken)
    {
        var currTime = DateTime.Now;
        var modeChangeTime = currTime.Date.AddHours(20);

        // Before 8:00 we just GET data.  After 8:00 we update daily low/high to account for aftermarket movement, and get anything new
        if (currTime < modeChangeTime)
        {
            await GetEndOfDayData(cToken);
        }
        else
        {
            await UpdateEndOfDayData(cToken);
        }
    }
}