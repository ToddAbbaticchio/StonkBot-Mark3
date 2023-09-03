using System.Diagnostics;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.SbActions._Enums;

namespace StonkBot.Services.SbActions;

public partial interface ISbAction
{
    Task UpdateCalculatedFields(CalcField field, CancellationToken cToken);
    Task UpdateCalculatedFields(CalcField field, bool recalcAll, CancellationToken cToken);
}

internal partial class SbAction
{
    public async Task UpdateCalculatedFields(CalcField field, CancellationToken cToken)
    {
        await UpdateCalculatedFields(field, false, cToken);
    }

    public async Task UpdateCalculatedFields(CalcField field, bool recalcAll, CancellationToken cToken)
    {
        switch (field)
        {
            case CalcField.FromYesterday:
                await CalculateFromYesterday(recalcAll, cToken);
                break;
            case CalcField.UpToday:
                await CalculateUpToday(recalcAll, cToken);
                break;
            case CalcField.VolumeAlert:
                await CalculateVolumeAlert(recalcAll, cToken);
                break;
            case CalcField.VolumeAlert2:
                await CalculateVolumeAlert2(recalcAll, cToken);
                break;
            case CalcField.FiveDayStable:
                await CalculateFiveDayStable(recalcAll, cToken);
                break;
            case CalcField.UpperShadow:
                await CalculateUpperShadow(recalcAll, cToken);
                break;
            case CalcField.AboveUpperShadow:
                await CalculateAboveUpperShadow(recalcAll, cToken);
                break;
            case CalcField.FhTargetDay:
                await CalculateFhTargetDay(recalcAll, cToken);
                break;
            case CalcField.LastFhTarget:
                await CalculateFhLastTarget(recalcAll, cToken);
                break;
            case CalcField.All:
                _con.WriteLog(MessageSeverity.Section, _targetLog, "SbActionRunner.UpdateCalculatedFields - Starting...");
                var calcAllTimer = new Stopwatch();
                calcAllTimer.Start();
                await CalculateFromYesterday(recalcAll, cToken);
                await CalculateUpToday(recalcAll, cToken);
                await CalculateVolumeAlert(recalcAll, cToken);
                await CalculateVolumeAlert2(recalcAll, cToken);
                await CalculateFiveDayStable(recalcAll, cToken);
                await CalculateUpperShadow(recalcAll, cToken);
                await CalculateAboveUpperShadow(recalcAll, cToken);
                await CalculateFhTargetDay(recalcAll, cToken);
                await CalculateFhLastTarget(recalcAll, cToken);
                calcAllTimer.Stop();
                _con.WriteLog(MessageSeverity.Info, _targetLog, $"Elapsed time: [{calcAllTimer.Elapsed}]");
                break;
            default:
                _con.WriteLog(MessageSeverity.Error, _targetLog, $"Provided field for calculation: {field} is invalid!");
                break;
        }
    }
}