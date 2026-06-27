using StrikeBox.Models;
using StrikeBox.Services.Runners;

namespace StrikeBox.Services;

public sealed class ToolExecutionService
{
    private readonly Dictionary<ToolType, IToolRunner> _runners;
    private readonly LogService _logService;

    public ToolExecutionService(IEnumerable<IToolRunner> runners, LogService logService)
    {
        _logService = logService;
        _runners = runners.ToDictionary(r => r.Type, r => r);
    }

    public bool Run(ToolItem tool)
    {
        try
        {
            if (_runners.TryGetValue(tool.Type, out var runner))
            {
                runner.Run(tool);
                _logService.Info($"运行工具成功: {tool.Name} ({tool.Type})");
                return true;
            }
            else
            {
                throw new InvalidOperationException($"未找到类型为 {tool.Type} 的运行器");
            }
        }
        catch (Exception ex)
        {
            _logService.Error($"运行工具失败 [{tool.Name}]: {ex.Message}");
            _ = ShowErrorAsync(ex.Message);
            return false;
        }
    }

    public static async Task ShowErrorAsync(string message)
    {
        var messageBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = "错误",
            Content = message,
            CloseButtonText = "确定",
        };
        await messageBox.ShowDialogAsync();
    }

    public static async Task<bool> ConfirmDeleteAsync(string title, string message)
    {
        var messageBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "确认",
            SecondaryButtonText = "取消",
        };
        var result = await messageBox.ShowDialogAsync();
        return result == Wpf.Ui.Controls.MessageBoxResult.Primary;
    }
}
