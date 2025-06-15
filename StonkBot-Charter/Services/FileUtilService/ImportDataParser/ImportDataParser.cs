using CsvHelper;
using CsvHelper.Configuration;
using StonkBotChartoMatic.Services.FileUtilService.Enums;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using StonkBotChartoMatic.Services.FileUtilService.Models;

namespace StonkBotChartoMatic.Services.FileUtilService.ImportDataParser;

public interface IImportDataParser
{
    bool TryParseCsv(string filePath, out ImportData? importData);
}

public class ImportDataParser : IImportDataParser
{
    public bool TryParseCsv(string filePath, out ImportData? importData)
    {
        try
        {
            // Read file (this is funny because it has to bypass a filelock from the main form after drag/drop)
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream, Encoding.Default);
            var rawString = streamReader.ReadToEnd()
                .Replace("\u00a0", "");
            using var stringReader = new StringReader(rawString);
            using var csvReader = new CsvReader(stringReader, new CsvConfiguration(CultureInfo.InvariantCulture));
            csvReader.Context.RegisterClassMap<ImportTransactionMap>();
            var tList = csvReader.GetRecords<ImportTransaction>().ToList();

            importData = new ImportData(tList);
            if (importData.EsTransactions.Count != 0 || importData.MesTransactions.Count != 0 || importData.NqTransactions.Count != 0 || importData.MnqTransactions.Count != 0)
                return true;
            
            importData = null;
            return false;
        }
        catch
        {
            importData = null;
            return false;
        }
    }
}