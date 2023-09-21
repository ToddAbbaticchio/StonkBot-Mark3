using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using StonkBotChartoMatic.Services.FileUtilService.Enums;
using StonkBotChartoMatic.Services.FileUtilService.Models;

namespace StonkBotChartoMatic.Services.FileUtilService.ImportDataParser;
public class ImportData
{
    public List<ImportTransaction> EsTransactions { get; set; } = new();
    public List<ImportTransaction> MesTransactions { get; set; } = new();
    public List<ImportTransaction> NqTransactions { get; set; } = new();
    public List<ImportTransaction> MnqTransactions { get; set; } = new();

    public ImportData(List<ImportTransaction> tList)
    {
        foreach (var t in tList.Where(x => x.Status != "Cancelled"))
        {
            if (t.Symbol.StartsWith("ES"))
                EsTransactions.Add(t);
            if (t.Symbol.StartsWith("MES"))
                MesTransactions.Add(t);
            if (t.Symbol.StartsWith("NQ"))
                NqTransactions.Add(t);
            if (t.Symbol.StartsWith("MNQ"))
                MnqTransactions.Add(t);
        }

        if (EsTransactions.Any())
            EsTransactions = DetermineOrderTypes(EsTransactions);
        if (MesTransactions.Any())
            MesTransactions = DetermineOrderTypes(MesTransactions);
        if (NqTransactions.Any())
            NqTransactions = DetermineOrderTypes(NqTransactions);
        if (MnqTransactions.Any())
            MnqTransactions = DetermineOrderTypes(MnqTransactions);
    }

    private static List<ImportTransaction> DetermineOrderTypes(List<ImportTransaction> tList)
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