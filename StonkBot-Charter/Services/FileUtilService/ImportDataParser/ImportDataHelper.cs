/*using StonkBotChartoMatic.Services.FileUtilService.Enums;
using StonkBotChartoMatic.Services.FileUtilService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace StonkBotChartoMatic.Services.FileUtilService.ImportDataParser;

public class ImportDataHelper
{
    public List<ImportTransaction> EsTransactions { get; set; } = new();
    public List<ImportTransaction> MesTransactions { get; set; } = new();
    public List<ImportTransaction> NqTransactions { get; set; } = new();
    public List<ImportTransaction> MnqTransactions { get; set; } = new();

    public static ImportDataHelper ProcessEntries(string allData)
    {
        var esList = new List<ImportTransaction>();
        var mesList = new List<ImportTransaction>();
        var nqList = new List<ImportTransaction>();
        var mnqList = new List<ImportTransaction>();

        foreach (var entry in )
        {
            try
            {
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
}*/