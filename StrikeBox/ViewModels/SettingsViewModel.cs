using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StrikeBox.Models;
using StrikeBox.Services;

namespace StrikeBox.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ConfigService _configService;

    [ObservableProperty]
    private string _pythonPath = string.Empty;

    [ObservableProperty]
    private string _java8Path = string.Empty;

    [ObservableProperty]
    private string _java11Path = string.Empty;

    [ObservableProperty]
    private string _java17Path = string.Empty;

    public SettingsViewModel(ConfigService configService)
    {
        _configService = configService;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var interpreters = _configService.Settings.Interpreters;
        PythonPath = interpreters.Python;
        Java8Path = interpreters.Java8;
        Java11Path = interpreters.Java11;
        Java17Path = interpreters.Java17;
    }

    [RelayCommand]
    private void BrowsePython()
    {
        PythonPath = BrowseFile("选择 Python 解释器", "Python 解释器|python.exe|所有文件|*.*") ?? PythonPath;
    }

    [RelayCommand]
    private void BrowseJava8()
    {
        Java8Path = BrowseFile("选择 Java 8 路径", "Java 可执行文件|java.exe|所有文件|*.*") ?? Java8Path;
    }

    [RelayCommand]
    private void BrowseJava11()
    {
        Java11Path = BrowseFile("选择 Java 11 路径", "Java 可执行文件|java.exe|所有文件|*.*") ?? Java11Path;
    }

    [RelayCommand]
    private void BrowseJava17()
    {
        Java17Path = BrowseFile("选择 Java 17 路径", "Java 可执行文件|java.exe|所有文件|*.*") ?? Java17Path;
    }

    private static string? BrowseFile(string title, string filter)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = title,
            Filter = filter
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    [RelayCommand]
    private async Task Save()
    {
        _configService.Settings.Interpreters.Python = PythonPath;
        _configService.Settings.Interpreters.Java8 = Java8Path;
        _configService.Settings.Interpreters.Java11 = Java11Path;
        _configService.Settings.Interpreters.Java17 = Java17Path;

        _configService.Save();

        var messageBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = "保存成功",
            Content = "设置已保存",
            CloseButtonText = "确定",
        };
        await messageBox.ShowDialogAsync();
    }
}
