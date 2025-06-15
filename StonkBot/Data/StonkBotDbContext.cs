using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StonkBot.Appsettings.Models;
using StonkBot.Data.Entities;
using StonkBot.Services.CharlesSchwab.APIClient;

namespace StonkBot.Data;

public interface IStonkBotDb
{
    // Move fast, break shit.  Guard rails are for suckers.
    public DbSet<AuthToken> AuthTokens { get; }
    public DbSet<EsCandle> EsCandles { get; }
    public DbSet<NqCandle> NqCandles { get; }
    public DbSet<HistoricalData> HistoricalData { get; }
    public DbSet<CalculatedFields> CalculatedFields { get; }
    public DbSet<IndustryInfo> IndustryInfo { get; }
    public DbSet<IndustryInfoHData> IndustryInfoHData { get; }
    public DbSet<IpoListing> IpoListings { get; }
    public DbSet<EarningsReport> EarningsReports { get; }
    public DbSet<DiscordMessageRecord> DiscordMessageRecords { get; }
    public DbSet<IpoHData> IpoHData { get; }
    public DbSet<WatchedSymbol> WatchedSymbols { get; }
    public DbSet<MostActive> MostActiveRecords { get; }

    Task SbSaveChangesAsync(CancellationToken cToken);
    Task<string> IsWatched(string symbol, CancellationToken cToken);
}

public partial class StonkBotDbContext : DbContext, IStonkBotDb
{
    public string DbPath { get; }
    
    public DbSet<AuthToken> AuthTokens { get; set; } = null!;
    public DbSet<EsCandle> EsCandles { get; set; } = null!;
    public DbSet<NqCandle> NqCandles { get; set; } = null!;
    public DbSet<HistoricalData> HistoricalData { get; set; } = null!;
    public DbSet<CalculatedFields> CalculatedFields { get; set; } = null!;
    public DbSet<IndustryInfo> IndustryInfo { get; set; } = null!;
    public DbSet<IndustryInfoHData> IndustryInfoHData { get; set; } = null!;
    public DbSet<IpoListing> IpoListings { get; set; } = null!;
    public DbSet<EarningsReport> EarningsReports { get; set; } = null!;
    public DbSet<DiscordMessageRecord> DiscordMessageRecords { get; set; } = null!;
    public DbSet<IpoHData> IpoHData { get; set; } = null!;
    public DbSet<WatchedSymbol> WatchedSymbols { get; set; } = null!;
    public DbSet<MostActive> MostActiveRecords { get; set; } = null!;

    public StonkBotDbContext(DbContextOptions<StonkBotDbContext> options, IOptions<DbConfig> dbConfig) : base(options)
    {
        var data = dbConfig.Value;
        var local = data.LocalDbFilePath;
        var network = data.NetworkDbFilePath;
        DbPath = File.Exists(local) ? local : network;
    }

    public StonkBotDbContext(IOptions<DbConfig> dbConfig)
    {
        var data = dbConfig.Value;
        var local = data.LocalDbFilePath;
        var network = data.NetworkDbFilePath;
        DbPath = File.Exists(local) ? local : network;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.access_token);
        });
        
        modelBuilder.Entity<CalculatedFields>(e =>
        {
            e.HasKey(x => new { x.Symbol, x.Date });
        });

        modelBuilder.Entity<DiscordMessageRecord>(e =>
        {
            e.HasKey(x => x.MessageId);
            e.HasIndex(x => new { x.Channel, x.DateTime });
        });

        modelBuilder.Entity<EarningsReport>(e =>
        {
            e.HasKey(x => new { x.Symbol, x.Date });
            e.HasMany(x => x.Alerts)
                .WithOne()
                .IsRequired(false);
        });

        modelBuilder.Entity<ErAlert>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<EsCandle>(e =>
        {
            e.HasKey(x => new { x.ChartTime });
        });

        modelBuilder.Entity<NqCandle>(e =>
        {
            e.HasKey(x => new { x.ChartTime });
        });

        modelBuilder.Entity<HistoricalData>(e =>
        {
            e.HasKey(x => new { x.Symbol, x.Date });
            e.HasOne(x => x.CalculatedFields)
                .WithMany()
                .IsRequired(false);
            e.HasOne(x => x.IndustryInfo)
                .WithMany()
                .IsRequired(false);
        });

        modelBuilder.Entity<IndustryInfo>(e =>
        {
            e.HasKey(x => x.Symbol);
        });

        modelBuilder.Entity<IndustryInfoHData>(e =>
        {
            e.HasKey(x => new { x.Symbol, x.Date });
            e.HasOne(x => x.IndustryInfo)
                .WithMany()
                .IsRequired(false);
        });

        modelBuilder.Entity<IpoHData>(e =>
        {
            e.HasKey(x => new { x.Symbol, x.Date });
        });

        modelBuilder.Entity<IpoListing>(e =>
        {
            e.HasKey(x => x.Symbol);
            e.HasMany(x => x.HData)
                .WithOne()
                .IsRequired(false);
        });

        modelBuilder.Entity<WatchedSymbol>(e =>
        {
            e.HasKey(x => x.Symbol);
        });

        modelBuilder.Entity<MostActive>(e =>
        {
            e.HasKey(x => new { x.Symbol, x.Date });
        });
    }

    public async Task SbSaveChangesAsync(CancellationToken cToken)
    {
        if (ChangeTracker.HasChanges())
        {
            await SaveChangesAsync(cToken);
            ChangeTracker.Clear();
        }
    }

    public async Task<string> IsWatched(string symbol, CancellationToken cToken) =>
        await WatchedSymbols.AnyAsync(x => x.Symbol == symbol, cToken) ? "WATCHED" : "";
}