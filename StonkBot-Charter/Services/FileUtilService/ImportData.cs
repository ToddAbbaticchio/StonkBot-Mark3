using StonkBotChartoMatic.Services.FileUtilService.Enums;
using StonkBotChartoMatic.Services.FileUtilService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace StonkBotChartoMatic.Services.FileUtilService;

public class ImportData
{
    public List<ImportTransaction> EsTransactions { get; set; } = new();
    public List<ImportTransaction> MesTransactions { get; set; } = new();
    public List<ImportTransaction> NqTransactions { get; set; } = new();
    public List<ImportTransaction> MnqTransactions { get; set; } = new();

    public static ImportData TryParse(string allData)
    {
        const string headerRow = "Symbol,Side,Type,Qty,Filled Qty,Limit Price,Stop Price,Avg Fill Price,Take Profit,Stop Loss,Status,Open Time,Close Time,Duration,Commission Fee,Expiration Date,Order ID";
        var esList = new List<ImportTransaction>();
        var mesList = new List<ImportTransaction>();
        var nqList = new List<ImportTransaction>();
        var mnqList = new List<ImportTransaction>();

        var lines = Regex.Split(allData, "\n").ToList();
        var lineCount = 0;
        foreach (var line in lines)
        {
            try
            {
                lineCount++;
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                if (line.Contains(headerRow))
                    continue;

                var values = line.Split(',');
                for (var i = 0; i < values.Length; i++)
                    values[i] = values[i].Replace("\u00a0", "");

                var thisTransaction = new ImportTransaction
                {
                    Symbol = values[0],
                    Side = values[1],
                    Type = values[2],
                    Qty = Convert.ToInt32(values[3]),
                    FilledQty = Convert.ToInt32(values[4]),
                    LimitPrice = string.IsNullOrEmpty(values[5]) ? decimal.Zero : Convert.ToDecimal(values[5]),
                    StopPrice = string.IsNullOrEmpty(values[6]) ? decimal.Zero : Convert.ToDecimal(values[6]),
                    AvgFillPrice = string.IsNullOrEmpty(values[7]) ? decimal.Zero : Convert.ToDecimal(values[7]),
                    Status = values[10],
                    OpenTime = Convert.ToDateTime(values[11]),
                    CloseTime = Convert.ToDateTime(values[12]),
                    Duration = values[13],
                    CommissionFee = string.IsNullOrEmpty(values[14]) ? decimal.Zero : Convert.ToDecimal(values[14]),
                    ExpirationDate = Convert.ToDateTime(values[15]),
                    OrderId = values[16],
                    OrderType = null
                };

                if (thisTransaction.Status == "Cancelled")
                    continue;

                if (thisTransaction.Symbol.StartsWith("ES"))
                    esList.Add(thisTransaction);
                if (thisTransaction.Symbol.StartsWith("MES"))
                    mesList.Add(thisTransaction);
                if (thisTransaction.Symbol.StartsWith("NQ"))
                    nqList.Add(thisTransaction);
                if (thisTransaction.Symbol.StartsWith("MNQ"))
                    mnqList.Add(thisTransaction);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing ImportTransaction on line {lineCount}: {ex.Message}");
            }
        }

        if (esList.Any())
            esList = DetermineOrderTypes(esList);
        if (mesList.Any())
            mesList = DetermineOrderTypes(mesList);
        if (nqList.Any())
            nqList = DetermineOrderTypes(nqList);
        if (mnqList.Any())
            mnqList = DetermineOrderTypes(mnqList);


        return new ImportData
        {
            EsTransactions = esList,
            MesTransactions = mesList,
            NqTransactions = nqList,
            MnqTransactions = mnqList
        };
    }

    internal static List<ImportTransaction> DetermineOrderTypes(List<ImportTransaction> tList)
    {
        // flatten list
        var flatList = new List<ImportTransaction>();
        foreach (var t in tList)
        {
            var saveQty = t.FilledQty;
            while (t.FilledQty >= 1)
            {
                flatList.Add(ImportTransaction.SplitTransaction(t, saveQty));
                t.FilledQty--;
            }
        }

        flatList = flatList.OrderBy(x => x.OpenTime).ToList();
        var outList = new List<ImportTransaction>();
        while (flatList.Any(x => x.OrderType == null))
        {
            var firstNull = flatList.First(x => x.OrderType == null);
            var findMatch = null as ImportTransaction;
            switch (firstNull.Side)
            {
                case "Buy":
                    findMatch = flatList
                        .Where(x => x.OrderType == null)
                        .Where(x => x.Side == "Sell")
                        .MinBy(x => x.OpenTime);

                    if (findMatch == null)
                    {
                        firstNull.OrderType = OrderType.NoMatch;
                        MessageBox.Show($@"Unable to find a match for order: {firstNull.OrderId}");
                        break;
                    }

                    firstNull.OrderType = OrderType.BuyToOpen;
                    firstNull.MatchedOrder = findMatch.OrderId;
                    findMatch.OrderType = OrderType.SellToClose;
                    findMatch.MatchedOrder = firstNull.OrderId;
                    break;

                case "Sell":
                    findMatch = flatList
                        .Where(x => x.OrderType == null)
                        .Where(x => x.Side == "Buy")
                        .MinBy(x => x.OpenTime);

                    if (findMatch == null)
                    {
                        firstNull.OrderType = OrderType.NoMatch;
                        MessageBox.Show($@"Unable to find a match for order: {firstNull.OrderId}");
                        break;
                    }

                    firstNull.OrderType = OrderType.SellToOpen;
                    firstNull.MatchedOrder = findMatch.OrderId;
                    findMatch.OrderType = OrderType.BuyToClose;
                    findMatch.MatchedOrder = firstNull.OrderId;
                    break;
            }
        }

        return flatList;
    }
}