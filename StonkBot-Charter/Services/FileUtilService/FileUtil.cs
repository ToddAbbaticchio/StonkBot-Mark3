using StonkBot.Data;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBotChartoMatic.Services.FileUtilService.Enums;
using StonkBotChartoMatic.Services.FileUtilService.Extensions;
using StonkBotChartoMatic.Services.FileUtilService.ImportDataParser;
using StonkBotChartoMatic.Services.FileUtilService.Models;
using StonkBotChartoMatic.Services.MapperService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StonkBotChartoMatic.Services.FileUtilService;

public interface IFileUtil
{
    Task<List<TCandle>> Import(string filePath, DateTime selectedDate, SbCharterMarket selectedMarket, CancellationToken cToken);
}

public class FileUtil : IFileUtil
{
    private readonly IStonkBotCharterDb _db;
    private readonly IMapperService _mapper;
    private readonly IImportDataParser _importDataParser;

    public FileUtil(IStonkBotCharterDb db, IMapperService mapper, IImportDataParser importDataParser)
    {
        _db = db;
        _mapper = mapper;
        _importDataParser = importDataParser;
    }

    public async Task<List<TCandle>> Import(string filePath, DateTime selectedDate, SbCharterMarket selectedMarket, CancellationToken cToken)
    {
        if (!_importDataParser.TryParseCsv(filePath, out var importData))
            throw new Exception($"Unable to parse provided .csv file: {filePath}");

        var transactionList = importData!.GetFullList();

        // make file for lazybutt Dan
        try
        {
            var fileName = $"Summary - {selectedDate:MMddyyyy}.csv";
            var outputFileData = await GetOutputFileData(importData!, selectedDate, cToken);
            await File.WriteAllLinesAsync($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{fileName}", outputFileData, Encoding.UTF8, cToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show($@"Error writing file: {ex.Message}");
        }

        var tCandleList = new List<TCandle>();
        if (transactionList.First().Symbol.Contains("ES"))
        {
            var esCandleList = await _db.GetEsCandles(selectedDate, selectedMarket, cToken);
            tCandleList.AddRange(_mapper.ConvertToTCandles(esCandleList, cToken));
        }
        if (transactionList.First().Symbol.Contains("NQ"))
        {
            var nqCandleList = await _db.GetNqCandles(selectedDate, selectedMarket, cToken);
            tCandleList.AddRange(_mapper.ConvertToTCandles(nqCandleList, cToken));
        }
        if (tCandleList.Count == 0)
            throw new Exception("Ahhhhhhh! No transactions!");


        // Insert transaction data into TCandles
        foreach (var t in transactionList)
        {
            var tCandle = tCandleList
                .Where(x => x.ChartTime == t.CloseTime.ToTheMinute())
                .FirstOrDefault();
            if (tCandle == null)
                continue;

            decimal? tPrice = null;
            if (t.AvgFillPrice != null && t.AvgFillPrice != decimal.Zero)
                tPrice = t.AvgFillPrice;
            if (t.StopPrice != null && t.StopPrice != decimal.Zero)
                tPrice = t.StopPrice;
            if (t.LimitPrice != null && t.LimitPrice != decimal.Zero)
                tPrice = t.LimitPrice;

            tCandle.Transactions.Add(new TCandleTransaction
            {
                ExecTime = t.CloseTime.ToTheMinute(),
                OrderType = t.OrderType ?? OrderType.NoMatch,
                Price = (decimal)tPrice!,
                Side = t.Side
            });
        }

        return tCandleList;
    }

    private Task<List<string>> GetOutputFileData(ImportData importData, DateTime selectedDate, CancellationToken cToken)
    {
        const string headerRow = "Symbol,Date,Day,Long or Short,Start Time,End Time,Time Frame,Enter Price,Close Price,Order Count,Close Count,Profit,Hold Time,Order Type,Reason(承接？追单？理由？技巧？市场价 还是limie order,,,ToOpenId,ToCloseId)";
        var toWrite = new List<string> { headerRow };
        decimal profitSum = 0;

        // Process ES transactions
        if (importData.EsTransactions.Count != 0)
        {
            var esToOpens = importData.EsTransactions
                .Where(x => x.OpenTime.Date == selectedDate.Date)
                .Where(x =>
                    x.OrderType == OrderType.BuyToOpen ||
                    x.OrderType == OrderType.SellToOpen)
                .ToList();

            var esToCloses = importData.EsTransactions
                .Where(x => x.OpenTime.Date == selectedDate.Date)
                .Where(x =>
                    x.OrderType == OrderType.BuyToClose ||
                    x.OrderType == OrderType.SellToClose)
                .ToList();

            foreach (var open in esToOpens)
            {
                var close = esToCloses.First(x => x.OrderId == open.MatchedOrder);
                var o = new OutputTransaction(open, close);
                toWrite.Add(o.GetOutputString(o));
                profitSum += o.Profit ?? decimal.Zero;
            }
        }

        // Process MES transactions
        if (importData.MesTransactions.Count != 0)
        {
            var mesToOpens = importData.MesTransactions
                .Where(x =>
                    x.OrderType == OrderType.BuyToOpen ||
                    x.OrderType == OrderType.SellToOpen)
                .ToList();

            var mesToCloses = importData.MesTransactions
                .Where(x =>
                    x.OrderType == OrderType.BuyToClose ||
                    x.OrderType == OrderType.SellToClose)
                .ToList();

            foreach (var open in mesToOpens)
            {
                var close = mesToCloses.First(x => x.OrderId == open.MatchedOrder);
                var o = new OutputTransaction(open, close);
                toWrite.Add(o.GetOutputString(o));
                profitSum += o.Profit ?? decimal.Zero;
            }
        }

        // Process NQ transactions
        if (importData.NqTransactions.Count != 0)
        {
            var nqToOpens = importData.NqTransactions
                .Where(x => x.OpenTime.Date == selectedDate.Date)
                .Where(x =>
                    x.OrderType == OrderType.BuyToOpen ||
                    x.OrderType == OrderType.SellToOpen)
                .ToList();

            var nqToCloses = importData.NqTransactions
                .Where(x => x.OpenTime.Date == selectedDate.Date)
                .Where(x =>
                    x.OrderType == OrderType.BuyToClose ||
                    x.OrderType == OrderType.SellToClose)
                .ToList();

            foreach (var open in nqToOpens)
            {
                var close = nqToCloses.First(x => x.OrderId == open.MatchedOrder);
                var o = new OutputTransaction(open, close);
                toWrite.Add(o.GetOutputString(o));
                profitSum += o.Profit ?? decimal.Zero;
            }
        }

        // Process MNQ transactions
        if (importData.MnqTransactions.Count != 0)
        {
            var mnqToOpens = importData.MnqTransactions
                .Where(x =>
                    x.OrderType == OrderType.BuyToOpen ||
                    x.OrderType == OrderType.SellToOpen)
                .ToList();

            var mnqToCloses = importData.MnqTransactions
                .Where(x =>
                    x.OrderType == OrderType.BuyToClose ||
                    x.OrderType == OrderType.SellToClose)
                .ToList();

            foreach (var open in mnqToOpens)
            {
                var close = mnqToCloses.First(x => x.OrderId == open.MatchedOrder);
                var o = new OutputTransaction(open, close);
                toWrite.Add(o.GetOutputString(o));
                profitSum += o.Profit ?? decimal.Zero;
            }
        }

        var message = profitSum > 0 ? "»-(¯`·.·´¯)-> $$$ <-(¯`·.·´¯)-«" : "(╯°□°)╯︵ ┻━┻";
        toWrite.Add($",,,,,,,,,,Total:,{profitSum},,{message},");

        return Task.FromResult(toWrite);
    }
}