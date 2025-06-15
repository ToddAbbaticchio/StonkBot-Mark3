using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;
using System.Data.SQLite;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using StonkBot.Appsettings.Models;

namespace StonkBot.Services.BackupService;

public interface IDbBackupService
{
    Task SbBackupChecks(CancellationToken cToken);
}

internal class DbBackupService : IDbBackupService
{
    private readonly IConsoleWriter _con;
    private readonly TargetLog _targetLog;
    private readonly DbConfig _dbConfig;

    public DbBackupService(IConsoleWriter con, IOptions<DbConfig> dbConfig)
    {
        _con = con;
        _targetLog = TargetLog.ActionRunner;
        _dbConfig = dbConfig.Value;
    }

    public async Task SbBackupChecks(CancellationToken cToken)
    {
        await BackupDbAsync(cToken);
        await PurgeAgedBackupsAsync(cToken);
    }

    private async Task BackupDbAsync(CancellationToken cToken)
    {
        // 17:30
        var backupFileName = $"{_dbConfig.DbBackupFolderPath}\\{DateTime.Today:yyyy-MM-dd}.db";
        if (File.Exists(backupFileName))
            return;

        _con.WriteLog(MessageSeverity.Section, _targetLog, "DbBackupService.BackupDbAsync - Starting...");
        var timer = new Stopwatch();
        timer.Start();
        
        await using var source = new SQLiteConnection($"Data Source={_dbConfig.LocalDbFilePath};Version=3;");
        await using var backup = new SQLiteConnection($"Data Source={backupFileName};Version=3;");
        
        await source.OpenAsync(cToken);
        await backup.OpenAsync(cToken);

        source.BackupDatabase(backup, "main", "main", -1, null, 0);
        await source.CloseAsync();
        await backup.CloseAsync();
        
        timer.Stop();
        _con.WriteLog(MessageSeverity.Info, _targetLog, $"Elapsed time: [{timer.Elapsed}]");
        _con.WriteLog(MessageSeverity.Stats, _targetLog, "DbBackupService.BackupDbAsync complete!");
    }

    private Task PurgeAgedBackupsAsync(CancellationToken cToken)
    {
        var backups = Directory.GetFiles(_dbConfig.DbBackupFolderPath)
            .OrderBy(x => x)
            .ToList();

        if (backups.Count <= _dbConfig.MaxDbBackupAge)
            return Task.CompletedTask;

        _con.WriteLog(MessageSeverity.Section, _targetLog, "DbBackupService.PurgeAgedBackupsAsync - Starting...");
        var timer = new Stopwatch();
        timer.Start();

        while (backups.Count > _dbConfig.MaxDbBackupAge)
        {
            var fName = Path.GetFileName(backups.First());
            _con.WriteLog(MessageSeverity.Info, _targetLog, $"Removing {fName}...");
            File.Delete(backups.First());
            backups.Remove(backups.First());
        }

        timer.Stop();
        _con.WriteLog(MessageSeverity.Info, _targetLog, $"Elapsed time: [{timer.Elapsed}]");
        _con.WriteLog(MessageSeverity.Stats, _targetLog, "DbBackupService.PurgeAgedBackupsAsync complete!");
        return Task.CompletedTask;
    }
}