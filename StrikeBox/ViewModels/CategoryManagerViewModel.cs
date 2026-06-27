using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using StrikeBox.Models;
using StrikeBox.Services;
using System.Collections.ObjectModel;

namespace StrikeBox.ViewModels;

public sealed partial class CategoryManagerViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<string> _categories = new();

    [ObservableProperty]
    private string _newCategory = string.Empty;

    public CategoryManagerViewModel(ConfigService configService, NavigationService navigationService)
    {
        _configService = configService;
        _navigationService = navigationService;
        RefreshCategories();
    }

    /// <summary>
    /// 每次进入分类管理页时清空输入框
    /// </summary>
    public void ResetNewCategoryInput()
    {
        NewCategory = string.Empty;
    }

    private void RefreshCategories()
    {
        Categories = new ObservableCollection<string>(_configService.GetCategories());
    }

    [RelayCommand]
    private async Task AddCategory()
    {
        if (string.IsNullOrWhiteSpace(NewCategory))
        {
            await ToolExecutionService.ShowErrorAsync("分类名称不能为空");
            return;
        }

        if (Categories.Contains(NewCategory.Trim()))
        {
            await ToolExecutionService.ShowErrorAsync("该分类已存在");
            return;
        }

        _configService.AddCategory(NewCategory.Trim());
        NewCategory = string.Empty;
        RefreshCategories();
        RefreshSidebarNav();
    }

    [RelayCommand]
    private async Task RemoveCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category)) return;

        if (category == "默认分类")
        {
            await ToolExecutionService.ShowErrorAsync("不能删除「默认分类」");
            return;
        }

        if (await ToolExecutionService.ConfirmDeleteAsync("确认删除",
            $"确定要删除分类「{category}」吗？\n该分类下的所有工具将移动到「默认分类」。"))
        {
            _configService.RemoveCategory(category);
            RefreshCategories();
            RefreshSidebarNav();
        }
    }

    private static void RefreshSidebarNav()
    {
        WeakReferenceMessenger.Default.Send(new CategoriesChangedMessage());
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.NavigateTo("Tools");
    }
}
