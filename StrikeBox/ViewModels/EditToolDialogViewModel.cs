using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StrikeBox.Models;
using StrikeBox.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace StrikeBox.ViewModels;

public sealed partial class EditToolDialogViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private string? _editingToolId;
    private IReadOnlyList<string> _existingCategories = Array.Empty<string>();

    public ToolItem? Result { get; private set; }

    public EditToolDialogViewModel(ConfigService configService)
    {
        _configService = configService;
    }

    [ObservableProperty]
    private string _dialogTitle = "添加工具";

    [ObservableProperty]
    private string _toolName = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "默认分类";

    [ObservableProperty]
    private ToolType _selectedType = ToolType.GUIEXE;

    [ObservableProperty]
    private string _toolPath = string.Empty;

    [ObservableProperty]
    private string _workDir = string.Empty;

    [ObservableProperty]
    private string _args = string.Empty;

    [ObservableProperty]
    private string? _selectedJavaVersion;

    [ObservableProperty]
    private string? _selectedPythonVersion;

    [ObservableProperty]
    private bool _isJavaType;

    [ObservableProperty]
    private bool _isPythonType;

    [ObservableProperty]
    private bool _isWebType;

    [ObservableProperty]
    private bool _isNotWebType = true;

    [ObservableProperty]
    private ObservableCollection<string> _categories = new();

    [ObservableProperty]
    private ObservableCollection<string> _javaVersions = new();

    [ObservableProperty]
    private ObservableCollection<string> _pythonVersions = new();

    [ObservableProperty]
    private ObservableCollection<ToolType> _toolTypes = new(
        Enum.GetValues<ToolType>());

    [ObservableProperty]
    private string _newCategory = string.Empty;

    partial void OnSelectedTypeChanged(ToolType value)
    {
        IsJavaType = value is ToolType.GUIJAVA or ToolType.TerminalJAVA;
        IsPythonType = value == ToolType.TerminalPYTHON;
        IsWebType = value == ToolType.WEB;
        IsNotWebType = value != ToolType.WEB;
    }

    public void Initialize(ToolItem? tool, IReadOnlyList<string> existingCategories)
    {
        _existingCategories = existingCategories;
        Categories = new ObservableCollection<string>(existingCategories);

        // 从配置加载可用的解释器版本列表
        var interpreters = _configService.Settings.Interpreters;

        JavaVersions = new ObservableCollection<string>(
            interpreters.JavaInterpreters.Select(j => j.Version));
        PythonVersions = new ObservableCollection<string>(
            interpreters.PythonInterpreters.Select(p => p.Version));

        if (tool == null)
        {
            // 新增模式
            DialogTitle = "添加工具";
            _editingToolId = null;
            ToolName = string.Empty;
            SelectedCategory = "默认分类";
            SelectedType = ToolType.GUIEXE;
            ToolPath = string.Empty;
            WorkDir = string.Empty;
            Args = string.Empty;
            SelectedJavaVersion = null;
            SelectedPythonVersion = null;
        }
        else
        {
            // 编辑模式
            DialogTitle = "编辑工具";
            _editingToolId = tool.Id;
            ToolName = tool.Name;
            SelectedCategory = tool.Category;
            SelectedType = tool.Type;
            ToolPath = tool.Path;
            WorkDir = tool.WorkDir;
            Args = tool.Args;
            SelectedJavaVersion = tool.JavaVersion;
            SelectedPythonVersion = tool.PythonVersion;
        }
    }

    [RelayCommand]
    private void BrowsePath()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "选择文件",
            Filter = "所有文件|*.*|可执行文件|*.exe|Python 脚本|*.py|JAR 文件|*.jar"
        };

        if (dialog.ShowDialog() == true)
        {
            ToolPath = dialog.FileName;
        }
    }

    [RelayCommand]
    private void BrowseWorkDir()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "选择文件（将使用其所在目录作为工作目录）",
            Filter = "所有文件|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            WorkDir = System.IO.Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
        }
    }

    [RelayCommand]
    private void AddNewCategory()
    {
        if (!string.IsNullOrWhiteSpace(NewCategory) && !Categories.Contains(NewCategory))
        {
            Categories.Add(NewCategory.Trim());
            SelectedCategory = NewCategory.Trim();
            NewCategory = string.Empty;
        }
    }

    [RelayCommand]
    private async Task Submit()
    {
        // 验证
        if (string.IsNullOrWhiteSpace(ToolName))
        {
            await ToolExecutionService.ShowErrorAsync("工具名称不能为空");
            return;
        }

        if (string.IsNullOrWhiteSpace(ToolPath))
        {
            await ToolExecutionService.ShowErrorAsync("工具路径不能为空");
            return;
        }

        if (SelectedType != ToolType.WEB && !File.Exists(ToolPath))
        {
            await ToolExecutionService.ShowErrorAsync($"工具文件不存在，请检查路径:\n{ToolPath}");
            return;
        }

        Result = new ToolItem
        {
            Id = _editingToolId ?? System.Guid.NewGuid().ToString(),
            Name = ToolName.Trim(),
            Category = SelectedCategory,
            Type = SelectedType,
            Path = ToolPath.Trim(),
            WorkDir = WorkDir.Trim(),
            Args = Args.Trim(),
            JavaVersion = IsJavaType ? SelectedJavaVersion : null,
            PythonVersion = IsPythonType ? SelectedPythonVersion : null
        };
    }

    [RelayCommand]
    private void Cancel()
    {
        Result = null;
    }
}
