﻿using Microsoft.EntityFrameworkCore;
using StonkBot.Data.Entities;
using StonkBot.Data.Enums;

namespace StonkBot.Data;

public interface IStonkBotCharterDb
{
    Task<List<EsCandle>> GetEsCandleRangeAsync(DateTime start, DateTime end, CancellationToken cToken);
    Task<List<EsCandle>> GetEsCandles(DateTime targetDate, SbCharterMarket targetMarket, CancellationToken cToken);
    Task<List<EsCandle>> GetTargetZoneCandles(DateTime start, DateTime end, CancellationToken cToken);


    Task<List<NqCandle>> GetNqCandleRangeAsync(DateTime start, DateTime end, CancellationToken cToken);
    Task<List<NqCandle>> GetNqCandles(DateTime targetDate, SbCharterMarket targetMarket, CancellationToken cToken);
}

public partial class StonkBotDbContext : IStonkBotCharterDb
{
    public async Task<List<EsCandle>> GetEsCandleRangeAsync(DateTime start, DateTime end, CancellationToken cToken)
    {
        return await EsCandles
            .Where(x => x.ChartTime >= start)
            .Where(x => x.ChartTime <= end)
            .AsNoTracking()
            .ToListAsync(cToken);
    }

    public async Task<List<EsCandle>> GetEsCandles(DateTime targetDate, SbCharterMarket targetMarket, CancellationToken cToken)
    {
        DateTime startTime;
        DateTime endTime;
        
        switch (targetMarket)
        {
            case SbCharterMarket.Day:
                startTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 9, 30, 00);
                endTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 17, 00, 00);
                return await GetEsCandleRangeAsync(startTime, endTime, cToken);

            case SbCharterMarket.Night:
                startTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.AddDays(-1).Day, 18, 00, 00);
                endTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 09, 29, 00);
                return await GetEsCandleRangeAsync(startTime, endTime, cToken);

            case SbCharterMarket.Both:
                startTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.AddDays(-1).Day, 18, 00, 00);
                endTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 17, 00, 00);
                return await GetEsCandleRangeAsync(startTime, endTime, cToken);

            default:
                throw new Exception($"Invalid SbCharterMarket supplied: {targetMarket}");
        }
    }

    public async Task<List<EsCandle>> GetTargetZoneCandles(DateTime start, DateTime end, CancellationToken cToken)
    {
        return await EsCandles
            .Where(x => x.ChartTime >= start)
            .Where(x => x.ChartTime <= end)
            .Where(x => x.ChartTime.Hour == 9 && x.ChartTime.Minute > 30 ||
                        x.ChartTime.Hour == 10 && x.ChartTime.Minute < 10 ||
                        x.ChartTime.Hour == 15 && x.ChartTime.Minute > 20)
            .AsNoTracking()
            .ToListAsync(cToken);
    }


    public async Task<List<NqCandle>> GetNqCandleRangeAsync(DateTime start, DateTime end, CancellationToken cToken)
    {
        return await NqCandles
            .Where(x => x.ChartTime >= start)
            .Where(x => x.ChartTime <= end)
            .AsNoTracking()
            .ToListAsync(cToken);
    }

    public async Task<List<NqCandle>> GetNqCandles(DateTime targetDate, SbCharterMarket targetMarket, CancellationToken cToken)
    {
        DateTime startTime;
        DateTime endTime;

        switch (targetMarket)
        {
            case SbCharterMarket.Day:
                startTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 9, 30, 00);
                endTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 17, 00, 00);
                return await GetNqCandleRangeAsync(startTime, endTime, cToken);

            case SbCharterMarket.Night:
                startTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.AddDays(-1).Day, 18, 00, 00);
                endTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 09, 29, 00);
                return await GetNqCandleRangeAsync(startTime, endTime, cToken);

            case SbCharterMarket.Both:
                startTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.AddDays(-1).Day, 18, 00, 00);
                endTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 17, 00, 00);
                return await GetNqCandleRangeAsync(startTime, endTime, cToken);

            default:
                throw new Exception($"Invalid SbCharterMarket supplied: {targetMarket}");
        }
    }

}