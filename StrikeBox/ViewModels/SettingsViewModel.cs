using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StrikeBox.Models;
using StrikeBox.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace StrikeBox.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ConfigService _configService;

    // ── Python 解释器 ──

    [ObservableProperty]
    private ObservableCollection<InterpreterEntry> _pythonInterpreters = new();

    [ObservableProperty]
    private string _newPythonVersion = string.Empty;

    [ObservableProperty]
    private string _newPythonPath = string.Empty;

    // ── Java 解释器 ──

    [ObservableProperty]
    private ObservableCollection<InterpreterEntry> _javaInterpreters = new();

    [ObservableProperty]
    private string _newJavaVersion = string.Empty;

    [ObservableProperty]
    private string _newJavaPath = string.Empty;

    public SettingsViewModel(ConfigService configService)
    {
        _configService = configService;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var interpreters = _configService.Settings.Interpreters;
        PythonInterpreters = new ObservableCollection<InterpreterEntry>(
            interpreters.PythonInterpreters.Select(p => new InterpreterEntry { Version = p.Version, Path = p.Path }));
        JavaInterpreters = new ObservableCollection<InterpreterEntry>(
            interpreters.JavaInterpreters.Select(j => new InterpreterEntry { Version = j.Version, Path = j.Path }));
    }

    // ── Python 操作 ──

    [RelayCommand]
    private void BrowsePythonEntry(InterpreterEntry? entry)
    {
        if (entry == null) return;
        entry.Path = BrowseFile($"选择 {entry.Version} 解释器", "Python 解释器|python.exe|所有文件|*.*") ?? entry.Path;
    }

    [RelayCommand]
    private void BrowseNewPythonPath()
    {
        NewPythonPath = BrowseFile("选择 Python 解释器", "Python 解释器|python.exe|所有文件|*.*") ?? NewPythonPath;
    }

    [RelayCommand]
    private void AddPythonInterpreter()
    {
        var version = NewPythonVersion.Trim();
        if (string.IsNullOrWhiteSpace(version))
        {
            _ = ToolExecutionService.ShowErrorAsync("版本名不能为空");
            return;
        }

        if (PythonInterpreters.Any(p => p.Version.Equals(version, System.StringComparison.OrdinalIgnoreCase)))
        {
            _ = ToolExecutionService.ShowErrorAsync("该版本已存在");
            return;
        }

        PythonInterpreters.Add(new InterpreterEntry { Version = version, Path = NewPythonPath.Trim() });
        NewPythonVersion = string.Empty;
        NewPythonPath = string.Empty;
    }

    [RelayCommand]
    private void RemovePythonInterpreter(InterpreterEntry? entry)
    {
        if (entry != null)
            PythonInterpreters.Remove(entry);
    }

    // ── Java 操作 ──

    [RelayCommand]
    private void BrowseJavaEntry(InterpreterEntry? entry)
    {
        if (entry == null) return;
        entry.Path = BrowseFile($"选择 {entry.Version} 路径", "Java 可执行文件|java.exe|所有文件|*.*") ?? entry.Path;
    }

    [RelayCommand]
    private void BrowseNewJavaPath()
    {
        NewJavaPath = BrowseFile("选择 Java 解释器", "Java 可执行文件|java.exe|所有文件|*.*") ?? NewJavaPath;
    }

    [RelayCommand]
    private void AddJavaInterpreter()
    {
        var version = NewJavaVersion.Trim();
        if (string.IsNullOrWhiteSpace(version))
        {
            _ = ToolExecutionService.ShowErrorAsync("版本名不能为空");
            return;
        }

        if (JavaInterpreters.Any(j => j.Version.Equals(version, System.StringComparison.OrdinalIgnoreCase)))
        {
            _ = ToolExecutionService.ShowErrorAsync("该版本已存在");
            return;
        }

        JavaInterpreters.Add(new InterpreterEntry { Version = version, Path = NewJavaPath.Trim() });
        NewJavaVersion = string.Empty;
        NewJavaPath = string.Empty;
    }

    [RelayCommand]
    private void RemoveJavaInterpreter(InterpreterEntry? entry)
    {
        if (entry != null)
            JavaInterpreters.Remove(entry);
    }

    // ── 保存 ──

    [RelayCommand]
    private async Task Save()
    {
        // 将编辑后的列表写回配置
        _configService.Settings.Interpreters.PythonInterpreters = PythonInterpreters
            .Select(p => new InterpreterEntry { Version = p.Version, Path = p.Path })
            .ToList();

        _configService.Settings.Interpreters.JavaInterpreters = JavaInterpreters
            .Select(j => new InterpreterEntry { Version = j.Version, Path = j.Path })
            .ToList();

        _configService.Save();

        var messageBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = "保存成功",
            Content = "设置已保存",
            CloseButtonText = "确定",
        };
        await messageBox.ShowDialogAsync();
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
}
