using Microsoft.EntityFrameworkCore;
using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.DiscordService.Enums;

namespace StonkBot.Services.SbActions;

internal partial class SbAction
{
    public async Task CheckSendFourHandAlert(DateTime targetDate, CancellationToken cToken)
    {
        const AlertType alertType = AlertType.FourHand;
        const string headerRow = "Sector,Industry,Category,Symbol";
        try
        {
            var alertData = await _db.HistoricalData
                .Where(x => x.Date == targetDate)
                .Include(x => x.CalculatedFields)
                .Where(x => x.CalculatedFields != null)
                .Where(x => x.CalculatedFields!.LastFHTarget != null)
                .Where(x => x.CalculatedFields!.LastFHTarget!.Contains(@","))
                .Include(x => x.IndustryInfo)
                .OrderBy(x => x.IndustryInfo!.Sector)
                .ThenBy(x => x.IndustryInfo!.Industry)
                .ThenBy(x => x.IndustryInfo!.Category)
                .AsSingleQuery()
                .ToListAsync(cToken);

            if (!alertData.Any())
                return;

            var tempFile = new List<string> { headerRow };

            foreach (var entry in alertData)
            {
                var previousDaysData = await _db.HistoricalData
                    .Where(x => x.Symbol == entry.Symbol)
                    .Where(x => x.Date < entry.Date)
                    .Include(x => x.CalculatedFields)
                    .Where(x => x.CalculatedFields != null)
                    .AsSingleQuery()
                .ToListAsync(cToken);

                HistoricalData? prevFhTargetDay = null;
                var targetRange = entry.CalculatedFields!.LastFHTarget!.Split(",");

                if (entry.CalculatedFields.FHTargetDay == "true")
                {
                    prevFhTargetDay ??= previousDaysData
                        .Where(x => x.CalculatedFields!.FHTargetDay == "true")
                        .MaxBy(x => x.Date);
                    if (prevFhTargetDay == null)
                        continue;

                    targetRange = prevFhTargetDay.CalculatedFields!.LastFHTarget!.Split(",");
                }

                var lowEnd = Convert.ToDecimal(targetRange[0]);
                var highEnd = Convert.ToDecimal(targetRange[1]);
                if (entry.Low <= lowEnd || entry.Low >= highEnd)
                    continue;

                prevFhTargetDay ??= previousDaysData
                    .Where(x => x.CalculatedFields!.FHTargetDay == "true")
                    .MaxBy(x => x.Date);

                // if we have a prevTargetDay, check for 'alert days' between then and now.  if we already hit that condition, bail
                if (prevFhTargetDay != null)
                {
                    var checkPrevAlerts = previousDaysData
                        .Where(x => x.Low > lowEnd)
                        .Where(x => x.Low < highEnd)
                        .Any(x => x.Date > prevFhTargetDay.Date);
                    if (checkPrevAlerts)
                        continue;
                }

                tempFile.Add($"{entry.IndustryInfo!.Sector},{entry.IndustryInfo.Industry},{entry.IndustryInfo.Category},{entry.Symbol}");
            }

            await _discordClient.SendFileAsync(DiscordChannel.FourHand, tempFile, targetDate, cToken);
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"Error proceessing alerts for {alertType} alerts on {targetDate.SbDateString()}: {ex.Message}");
        }
    }
}