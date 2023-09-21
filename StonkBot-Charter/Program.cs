using Microsoft.Extensions.DependencyInjection;
using StonkBot.Data;
using StonkBotChartoMatic.Services.DbConnService;
using StonkBotChartoMatic.Services.FileUtilService;
using StonkBotChartoMatic.Services.FileUtilService.ImportDataParser;
using StonkBotChartoMatic.Services.MapperService;
using StonkBotChartoMatic.Services.MapperService.Mappers;
using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StonkBotChartoMatic;

[SupportedOSPlatform("windows10.0.17763.0")]
public static class Program
{
    [STAThread]
    private static async Task Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
            
        var services = new ServiceCollection();
        ConfigureServices(services);
            
        await using var serviceProvider = services.BuildServiceProvider();
        var charterForm = serviceProvider.GetRequiredService<CharterForm>();

        var cancellationTokenSource = new CancellationTokenSource();
        charterForm.FormClosed += (sender, e) =>
        {
            cancellationTokenSource.Cancel();
            Environment.Exit(0);
        };

        // Start the application asynchronously
        await RunApplicationAsync(charterForm, cancellationTokenSource.Token);
    }

    private static async Task RunApplicationAsync(CharterForm charterForm, CancellationToken cToken)
    {
        var completionSource = new TaskCompletionSource<bool>();

        // Subscribe to the Application.ThreadExit event to signal completion
        Application.ThreadExit += (sender, args) => completionSource.SetResult(true);

        // Start the application on a separate UI thread
        var thread = new Thread(() =>
        {
            Application.Run(charterForm);
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        // Wait for the application to exit
        await Task.WhenAny(completionSource.Task, Task.Delay(Timeout.Infinite, cToken));
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<CharterForm>();
        services.AddTransient<IStonkBotCharterDb, StonkBotDbContext>();
        services.AddTransient<IDbConn, DbConn>();
        services.AddTransient<IFileUtil,  FileUtil>();
        services.AddTransient<IImportDataParser, ImportDataParser>();
        services.AddTransient<IMapperService, MapperService>();
        services.AddAutoMapper(typeof(SbCharterMapper));
    }
}