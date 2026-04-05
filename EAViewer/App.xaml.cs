using System.IO;
using System.Windows;
using EAViewer.Core.Interfaces;
using EAViewer.Core.Repositories;
using EAViewer.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EAViewer;

public partial class App : Application
{
    private static IServiceProvider? _serviceProvider;

    public static IServiceProvider Services =>
        _serviceProvider ?? throw new InvalidOperationException("Service provider not initialized.");

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");

        // Setup DI
        var services = new ServiceCollection();

        services.AddSingleton<ITableRepository>(_ => new TableRepository(connectionString));
        services.AddSingleton<IBackgroundColorRepository>(_ => new BackgroundColorRepository(connectionString));
        services.AddSingleton<TableService>();

        _serviceProvider = services.BuildServiceProvider();

        // Create and show main window
        var tableService = _serviceProvider.GetRequiredService<TableService>();
        var mainWindow = new Views.MainWindow(tableService);
        mainWindow.Show();
    }
}
