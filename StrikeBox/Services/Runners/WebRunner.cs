using StrikeBox.Models;
using System.Diagnostics;

namespace StrikeBox.Services.Runners;

public sealed class WebRunner : IToolRunner
{
    public ToolType Type => ToolType.WEB;

    public void Run(ToolItem tool)
    {
        if (string.IsNullOrWhiteSpace(tool.Path))
            throw new InvalidOperationException("URL 不能为空");

        var url = tool.Path.Trim();

        // 自动补全 https://
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://" + url;
        }

        var psi = new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        };

        Process.Start(psi);
    }
}
