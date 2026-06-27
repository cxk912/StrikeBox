using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using StrikeBox.Models;
using StrikeBox.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace StrikeBox.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly NavigationService _navigationService;
    private readonly ConfigService _configService;
    private readonly ToolsViewModel _toolsViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly CategoryManagerViewModel _categoryManagerViewModel;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _currentPageName = "全部工具";

    [ObservableProperty]
    private ObservableCollection<NavItem> _navItems = new();

    [ObservableProperty]
    private NavItem? _selectedNavItem;

    private bool _isHandlingSelection;

    public MainWindowViewModel(
        NavigationService navigationService,
        ConfigService configService,
        ToolsViewModel toolsViewModel,
        SettingsViewModel settingsViewModel,
        CategoryManagerViewModel categoryManagerViewModel)
    {
        _navigationService = navigationService;
        _configService = configService;
        _toolsViewModel = toolsViewModel;
        _settingsViewModel = settingsViewModel;
        _categoryManagerViewModel = categoryManagerViewModel;

        _navigationService.NavigationRequested += OnNavigationRequested;

        // 订阅分类变更消息，自动刷新左侧导航栏
        WeakReferenceMessenger.Default.Register<CategoriesChangedMessage>(this, (_, _) => RefreshNavItems());

        // 初始化导航菜单
        RefreshNavItems();

        // 默认选中「全部工具」
        _isHandlingSelection = true;
        SelectedNavItem = NavItems.FirstOrDefault(n => n.Tag == "Tools");
        _isHandlingSelection = false;

        // 默认显示工具页
        CurrentView = toolsViewModel;
        CurrentPageName = "全部工具";
    }

    /// <summary>
    /// 刷新左侧导航中的分类项
    /// </summary>
    public void RefreshNavItems()
    {
        var categories = _configService.GetCategories();

        NavItems.Clear();
        NavItems.Add(new NavItem { Label = "全部工具", Tag = "Tools" });

        foreach (var cat in categories)
        {
            NavItems.Add(new NavItem { Label = cat, Tag = $"Cat:{cat}" });
        }

        NavItems.Add(new NavItem { Label = "管理分类", Tag = "Categories" });
        NavItems.Add(new NavItem { Label = "设置", Tag = "Settings" });
    }

    /// <summary>
    /// 按分类筛选工具（不离开工具页）
    /// </summary>
    public void NavigateToCategory(string category)
    {
        RefreshNavItems();

        // 同步左侧导航选中状态
        _isHandlingSelection = true;
        var tagToFind = string.IsNullOrEmpty(category) || category == "全部" ? "Tools" : $"Cat:{category}";
        SelectedNavItem = NavItems.FirstOrDefault(n => n.Tag == tagToFind);
        _isHandlingSelection = false;

        _toolsViewModel.SelectedCategory = category;
        _toolsViewModel.Refresh();

        if (!ReferenceEquals(CurrentView, _toolsViewModel))
        {
            CurrentView = _toolsViewModel;
        }

        CurrentPageName = string.IsNullOrEmpty(category) || category == "全部"
            ? "全部工具"
            : category;
    }

    partial void OnSelectedNavItemChanged(NavItem? value)
    {
        if (_isHandlingSelection || value == null) return;

        _isHandlingSelection = true;
        try
        {
            var tag = value.Tag;
            if (tag.StartsWith("Cat:"))
            {
                NavigateToCategory(tag[4..]);
            }
            else
            {
                _navigationService.NavigateTo(tag);
            }
        }
        finally
        {
            _isHandlingSelection = false;
        }
    }

    private void OnNavigationRequested(string pageKey)
    {
        // 每次导航前刷新分类列表，确保新增/删除的分类即时生效
        RefreshNavItems();

        _isHandlingSelection = true;
        try
        {
            switch (pageKey)
            {
                case "Tools":
                    _toolsViewModel.SelectedCategory = "全部";
                    _toolsViewModel.Refresh();
                    CurrentView = _toolsViewModel;
                    CurrentPageName = "全部工具";
                    SelectedNavItem = NavItems.FirstOrDefault(n => n.Tag == "Tools");
                    break;

                case "Settings":
                    CurrentView = _settingsViewModel;
                    CurrentPageName = "设置";
                    SelectedNavItem = NavItems.FirstOrDefault(n => n.Tag == "Settings");
                    break;

                case "Categories":
                    _categoryManagerViewModel.ResetNewCategoryInput();
                    CurrentView = _categoryManagerViewModel;
                    CurrentPageName = "分类管理";
                    SelectedNavItem = NavItems.FirstOrDefault(n => n.Tag == "Categories");
                    break;
            }
        }
        finally
        {
            _isHandlingSelection = false;
        }
    }
}
