using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MsOptions = Microsoft.Extensions.Options;
using StonkBot.Appsettings.Models;
using System.Reflection;
using StonkBot.Services.CharlesSchwab.APIClient;

namespace StonkBot.Data;
public class StonkBotDbContextFactory : IDesignTimeDbContextFactory<StonkBotDbContext>
{
    public StonkBotDbContext CreateDbContext(string[] args)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("appsettings.json");

        if (stream == null)
        {
            throw new FileNotFoundException("Could not find appsettings.json embedded resource");
        }

        using var reader = new StreamReader(stream);
        var fileContents = reader.ReadToEnd();

        var configuration = new ConfigurationBuilder()
             .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContents)))
             .Build();

        var dbConfig = new DbConfig();
        configuration.GetSection("DbConfig").Bind(dbConfig);

        var localDbPath = dbConfig.LocalDbFilePath;
        var networkDbPath = dbConfig.NetworkDbFilePath;
        var dbPath = File.Exists(localDbPath) ? localDbPath : networkDbPath;

        var optionsBuilder = new DbContextOptionsBuilder<StonkBotDbContext>();
        optionsBuilder.UseSqlite($"Data source={dbPath}");
        return new StonkBotDbContext(MsOptions.Options.Create(dbConfig));
    }
}
