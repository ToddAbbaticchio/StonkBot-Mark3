using Microsoft.EntityFrameworkCore;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.DiscordService.Enums;

namespace StonkBot.Services.SbActions;

internal partial class SbAction
{
    public async Task CheckSendUpperShadowAlert(DateTime targetDate, CancellationToken cToken)
    {
        const AlertType alertType = AlertType.UpperShadow;
        const string headerRow = "Sector,Industry,Category,Symbol";
        try
        {
            var alertData = await _db.HistoricalData
                .Where(x => x.Date == targetDate)
                .Include(x => x.CalculatedFields)
                .Where(x => x.CalculatedFields != null)
                .Where(x => x.CalculatedFields!.AboveUpperShadow == "firsttrue")
                .Include(x => x.IndustryInfo)
                .OrderBy(x => x.IndustryInfo!.Sector)
                .ThenBy(x => x.IndustryInfo!.Industry)
                .ThenBy(x => x.IndustryInfo!.Category)
                .AsSingleQuery()
                .ToListAsync(cToken);

            if (!alertData.Any())
                return;

            var tempFile = new List<string> { headerRow };
            tempFile.AddRange(alertData.Select(entry => $"{entry.IndustryInfo!.Sector},{entry.IndustryInfo.Industry},{entry.IndustryInfo.Category},{entry.Symbol}"));
            await _discordClient.SendFileAsync(DiscordChannel.UpperShadow, tempFile, targetDate, cToken);
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"Error proceessing alerts for {alertType} alerts on {targetDate.SbDateString()}: {ex.Message}");
        }
    }
}