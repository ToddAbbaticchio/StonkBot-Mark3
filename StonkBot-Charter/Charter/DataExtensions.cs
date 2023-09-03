using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using StonkBotChartoMatic.Services.FileUtilService;
using StonkBotChartoMatic.Services.FileUtilService.Models;

namespace StonkBotChartoMatic.Charter;
public static class DataExtensions
{
    public static DataTable ToDataTable<T>(this IList<T> data)
    {
        var props = TypeDescriptor.GetProperties(typeof(T));
        var table = new DataTable();
        
        for (var i = 0; i < props.Count; i++)
        {
            var prop = props[i];
            table.Columns.Add(prop.Name, prop.PropertyType);
        }
        var values = new object[props.Count];
        foreach (var item in data)
        {
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = props[i].GetValue(item)!;
            }
            table.Rows.Add(values);
        }
        return table;
    }

    public static List<ImportTransaction> GetFullList(this ImportData input, DateTime selectedDate)
    {
        var transactionList = new List<ImportTransaction>();
        
        transactionList.AddRange(input.EsTransactions
            .Where(x => x.OpenTime.Date == selectedDate.Date)
            .ToList());
        transactionList.AddRange(input.MesTransactions
            .Where(x => x.OpenTime.Date == selectedDate.Date)
            .ToList());

        transactionList.AddRange(input.NqTransactions
            .Where(x => x.OpenTime.Date == selectedDate.Date)
            .ToList());
        transactionList.AddRange(input.MnqTransactions
            .Where(x => x.OpenTime.Date == selectedDate.Date)
            .ToList());

        return transactionList;
    }
}