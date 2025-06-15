using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.DiscordService.Enums;

namespace StonkBot.Services.SbActions;

public partial interface ISbAction
{
    Task DiscordAlert(AlertType type, CancellationToken cToken);
    Task DiscordAlert(AlertType type, DateTime targetDate, CancellationToken cToken);
}

internal partial class SbAction
{
    public async Task DiscordAlert(AlertType type, CancellationToken cToken)
    {
        await DiscordAlert(type, DateTime.Today.SbDate(), cToken);
    }

    public async Task DiscordAlert(AlertType type, DateTime targetDate, CancellationToken cToken)
    {
        var dbMessageRecords = _db.DiscordMessageRecords;

        switch (type)
        {
            case AlertType.Volume:
            {
                var checkVol = await dbMessageRecords
                    .Where(x => x.DateTime == targetDate)
                    .Where(x => x.Channel == DiscordChannel.VolAlert.ToString())
                    .FirstOrDefaultAsync(cToken);

                if (checkVol != null)
                    await CheckSendVolumeAlert(targetDate, cToken);
                break;
            }
            case AlertType.Volume2:
            {
                var checkVol2 = await dbMessageRecords
                    .Where(x => x.DateTime == targetDate)
                    .Where(x => x.Channel == DiscordChannel.VolAlert2.ToString())
                    .FirstOrDefaultAsync(cToken);

                if (checkVol2 == null)
                    await CheckSendVolumeAlert2(targetDate, cToken);
                break;
            }
            case AlertType.UpperShadow:
            {
                var checkUs = await dbMessageRecords
                    .Where(x => x.DateTime == targetDate)
                    .Where(x => x.Channel == DiscordChannel.UpperShadow.ToString())
                    .FirstOrDefaultAsync(cToken);

                if (checkUs == null)
                    await CheckSendUpperShadowAlert(targetDate, cToken);
                break;
            }
            case AlertType.FourHand:
            {
                var check4H = await dbMessageRecords
                    .Where(x => x.DateTime == targetDate)
                    .Where(x => x.Channel == DiscordChannel.FourHand.ToString())
                    .FirstOrDefaultAsync(cToken);

                if (check4H == null)
                    await CheckSendFourHandAlert(targetDate, cToken);
                break;
            }
            case AlertType.AllDailyAlerts:
            {
                _con.WriteLog(MessageSeverity.Section, _targetLog, "SbActionRunner.DailyAlerts - Starting...");
                var timer = new Stopwatch();
                timer.Start();
                
                var todayMessages = await dbMessageRecords
                    .Where(x => x.DateTime == targetDate)
                    .ToListAsync(cToken);

                if (todayMessages.Count == 0 || todayMessages.All(x => x.Channel != DiscordChannel.VolAlert.ToString()))
                {
                    _con.WriteLog(MessageSeverity.Info, _targetLog, "  Processing DB for VolumeAlerts...");
                    await CheckSendVolumeAlert(targetDate, cToken);
                }

                if (todayMessages.Count == 0 || todayMessages.All(x => x.Channel != DiscordChannel.VolAlert2.ToString()))
                {
                    _con.WriteLog(MessageSeverity.Info, _targetLog, "  Processing DB for VolumeAlerts2...");
                    await CheckSendVolumeAlert2(targetDate, cToken);
                }

                if (todayMessages.Count == 0 || todayMessages.All(x => x.Channel != DiscordChannel.UpperShadow.ToString()))
                {
                    _con.WriteLog(MessageSeverity.Info, _targetLog, "  Processing DB for UpperShadowAlerts...");
                    await CheckSendUpperShadowAlert(targetDate, cToken);
                }
                
                if (todayMessages.Count == 0 || todayMessages.All(x => x.Channel != DiscordChannel.FourHand.ToString()))
                {
                    _con.WriteLog(MessageSeverity.Info, _targetLog, "  Processing DB for FourHandAlerts...");
                    await CheckSendFourHandAlert(targetDate, cToken);
                }
                
                timer.Stop();
                _con.WriteLog(MessageSeverity.Info, _targetLog, $"Elapsed time: [{timer.Elapsed}]");

                break;
            }
            default:
            {
                _con.WriteLog(MessageSeverity.Error, $"Provided alert type : {type} is invalid!");
                break;
            }
        }
    }
}