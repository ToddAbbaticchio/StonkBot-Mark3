using StonkBot.Data;
using System.Runtime.Versioning;

namespace StonkBot_FE;

public static class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews();
        builder.Services.AddTransient<IStonkBotCharterDb, StonkBotDbContext>();
        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
            app.UseHsts();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapControllers();
        app.MapFallbackToFile("index.html");
        await app.RunAsync();
    }
}