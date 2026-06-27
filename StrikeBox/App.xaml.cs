using Microsoft.Extensions.DependencyInjection;
using StrikeBox.Services;
using StrikeBox.Services.Runners;
using StrikeBox.ViewModels;
using System.Windows;

namespace StrikeBox;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<LogService>();
        services.AddSingleton<ConfigService>();
        services.AddSingleton<ToolExecutionService>();
        services.AddSingleton<NavigationService>();

        // Runners
        services.AddSingleton<IToolRunner, GuiExeRunner>();
        services.AddSingleton<IToolRunner, GuiJavaRunner>();
        services.AddSingleton<IToolRunner, TerminalExeRunner>();
        services.AddSingleton<IToolRunner, TerminalPythonRunner>();
        services.AddSingleton<IToolRunner, TerminalJavaRunner>();
        services.AddSingleton<IToolRunner, WebRunner>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<ToolsViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<CategoryManagerViewModel>();
        services.AddTransient<EditToolDialogViewModel>();

        // Windows and Views
        services.AddSingleton<MainWindow>();

        Services = services.BuildServiceProvider();

        // 加载配置
        Services.GetRequiredService<ConfigService>().Load();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }
}
