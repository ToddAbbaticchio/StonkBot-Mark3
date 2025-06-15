namespace StonkBot.Appsettings.Models;

public class DbConfig
{
    public string LocalDbFilePath { get; set; } = null!;
    public string NetworkDbFilePath { get; set; } = null!;
    public string DbBackupFolderPath { get; set; } = null!;
    public int MaxDbBackupAge { get; set; }
}