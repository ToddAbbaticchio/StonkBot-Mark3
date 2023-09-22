using StonkBotChartoMatic.Services.FileUtilService.ImportDataParser;
using StonkBotChartoMatic.Services.FileUtilService.Models;
using System.Collections.Generic;

namespace StonkBotChartoMatic.Services.FileUtilService.Extensions;

public static class FileUtilExtensions
{
    public static List<ImportTransaction> GetFullList(this ImportData x)
    {
        var combinedList = new List<ImportTransaction>();
        combinedList.AddRange(x.EsTransactions);
        combinedList.AddRange(x.MesTransactions);
        combinedList.AddRange(x.NqTransactions);
        combinedList.AddRange(x.MnqTransactions);

        return combinedList;
    }
}