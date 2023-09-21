using CsvHelper.Configuration;
using StonkBotChartoMatic.Services.FileUtilService.Models;

namespace StonkBotChartoMatic.Services.FileUtilService.ImportDataParser;

public sealed class ImportTransactionMap : ClassMap<ImportTransaction>
{
    public ImportTransactionMap()
    {
        Map<ImportTransaction>(x => x.Symbol);
        Map<ImportTransaction>(x => x.Side);
        Map<ImportTransaction>(x => x.Type);
        Map<ImportTransaction>(x => x.Qty);
        Map<ImportTransaction>(x => x.Duration);
        Map<ImportTransaction>(x => x.Status).Optional();

        Map<ImportTransaction>(x => x.FilledQty).Name("Filled Qty");
        Map<ImportTransaction>(x => x.LimitPrice).Name("Limit Price");
        Map<ImportTransaction>(x => x.StopPrice).Name("Stop Price");
        Map<ImportTransaction>(x => x.AvgFillPrice).Name("Avg Fill Price");
        Map<ImportTransaction>(x => x.OpenTime).Name("Open Time");
        Map<ImportTransaction>(x => x.CloseTime).Name("Close Time");
        Map<ImportTransaction>(x => x.CommissionFee).Name("Commission Fee");
        Map<ImportTransaction>(x => x.ExpirationDate).Name("Expiration Date");
        Map<ImportTransaction>(x => x.OrderId).Name("Order ID");
    }
}