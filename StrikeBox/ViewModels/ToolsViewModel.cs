using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using StrikeBox.Models;
using StrikeBox.Services;
using StrikeBox.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace StrikeBox.ViewModels;

public sealed partial class ToolsViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private readonly ToolExecutionService _executionService;
    private readonly NavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;
    private List<ToolItem> _allTools = new();
    /// <summary>
    /// 与 App.xaml &lt;ui:ThemesDictionary Theme="Dark" /&gt; 保持同步。
    /// </summary>
    private bool _isDarkTheme = true;

    [ObservableProperty]
    private ObservableCollection<ToolItem> _tools = new();

    [ObservableProperty]
    private ObservableCollection<string> _categories = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "全部";

    [ObservableProperty]
    private string _themeButtonText;

    public ToolsViewModel(
        ConfigService configService,
        ToolExecutionService executionService,
        NavigationService navigationService,
        IServiceProvider serviceProvider)
    {
        _configService = configService;
        _executionService = executionService;
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;
        ThemeButtonText = _isDarkTheme ? "☀" : "🌙";
        Refresh();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allTools.AsEnumerable();

        // 分类筛选
        if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "全部")
        {
            filtered = filtered.Where(t => t.Category == SelectedCategory);
        }

        // 搜索筛选
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(t =>
                t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        Tools = new ObservableCollection<ToolItem>(filtered);
    }

    [RelayCommand]
    private void RunTool(ToolItem? tool)
    {
        if (tool != null)
            _executionService.Run(tool);
    }

    [RelayCommand]
    private void AddTool()
    {
        var dialogVm = _serviceProvider.GetRequiredService<EditToolDialogViewModel>();
        dialogVm.Initialize(null, _configService.GetCategories());

        if (ShowEditDialog(dialogVm) == true && dialogVm.Result != null)
        {
            _configService.AddTool(dialogVm.Result);
            Refresh();
        }
    }

    [RelayCommand]
    private void EditTool(ToolItem? tool)
    {
        if (tool == null) return;

        var dialogVm = _serviceProvider.GetRequiredService<EditToolDialogViewModel>();
        dialogVm.Initialize(tool, _configService.GetCategories());

        if (ShowEditDialog(dialogVm) == true && dialogVm.Result != null)
        {
            _configService.UpdateTool(dialogVm.Result);
            Refresh();
        }
    }

    [RelayCommand]
    private async Task DeleteTool(ToolItem? tool)
    {
        if (tool == null) return;

        if (await ToolExecutionService.ConfirmDeleteAsync("确认删除", $"确定要删除工具「{tool.Name}」吗？\n此操作不可撤销。"))
        {
            _configService.RemoveTool(tool.Id);
            Refresh();
        }
    }

    [RelayCommand]
    private void RefreshCommand()
    {
        Refresh();
    }

    public void Refresh()
    {
        _allTools = _configService.GetTools().ToList();
        Categories = new ObservableCollection<string>(
            new[] { "全部" }.Concat(_configService.GetCategories()));
        ApplyFilters();
    }

    private static bool? ShowEditDialog(EditToolDialogViewModel dialogVm)
    {
        var dialog = new EditToolDialog(dialogVm);
        dialog.Owner = App.Current.MainWindow;
        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        return dialog.ShowDialog();
    }

    [RelayCommand]
    private void ManageCategories()
    {
        _navigationService.NavigateTo("Categories");
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        _isDarkTheme = !_isDarkTheme;
        if (_isDarkTheme)
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Mica, true);
            ThemeButtonText = "☀";
        }
        else
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Light, WindowBackdropType.Mica, true);
            ThemeButtonText = "🌙";
        }
    }
}
