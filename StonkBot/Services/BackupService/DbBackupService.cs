﻿using StonkBot.Services.ConsoleWriter.Enums;
using System.Data.SQLite;
using System.Diagnostics;
using StonkBot.Services.ConsoleWriter;

namespace StonkBot.Services.BackupService;

public interface IDbBackupService
{
    Task SbBackupChecks(CancellationToken cToken);
    Task BackupDbAsync(CancellationToken cToken);
    Task PurgeAgedBackupsAsync(CancellationToken cToken);
}

internal class DbBackupService : IDbBackupService
{
    private readonly IConsoleWriter _con;
    private readonly TargetLog _targetLog;

    public DbBackupService(IConsoleWriter con)
    {
        _con = con;
        _targetLog = TargetLog.ActionRunner;
    }

    public async Task SbBackupChecks(CancellationToken cToken)
    {
        await BackupDbAsync(cToken);
        await PurgeAgedBackupsAsync(cToken);
    }

    public async Task BackupDbAsync(CancellationToken cToken)
    {
        // 17:30
        var backupFileName = $"{Constants.DbBackupFolderPath}\\{DateTime.Today:yyyy-MM-dd}.db";
        if (File.Exists(backupFileName))
            return;

        _con.WriteLog(MessageSeverity.Section, _targetLog, "DbBackupService.BackupDbAsync - Starting...");
        var timer = new Stopwatch();
        timer.Start();
        
        await using var source = new SQLiteConnection($"Data Source={Constants.LocalDbFilePath};Version=3;");
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

    public async Task PurgeAgedBackupsAsync(CancellationToken cToken)
    {
        var backups = Directory.GetFiles(Constants.DbBackupFolderPath)
            .OrderBy(x => x)
            .ToList();

        if (backups.Count <= Constants.MaxDbBackupAge)
            return;

        _con.WriteLog(MessageSeverity.Section, _targetLog, "DbBackupService.PurgeAgedBackupsAsync - Starting...");
        var timer = new Stopwatch();
        timer.Start();

        while (backups.Count > Constants.MaxDbBackupAge)
        {
            var fName = Path.GetFileName(backups.First());
            _con.WriteLog(MessageSeverity.Info, _targetLog, $"Removing {fName}...");
            File.Delete(backups.First());
            backups.Remove(backups.First());
        }

        timer.Stop();
        _con.WriteLog(MessageSeverity.Info, _targetLog, $"Elapsed time: [{timer.Elapsed}]");
        _con.WriteLog(MessageSeverity.Stats, _targetLog, "DbBackupService.PurgeAgedBackupsAsync complete!");
    }
}