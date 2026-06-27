using StrikeBox.Models;
using System.Diagnostics;
using System.IO;

namespace StrikeBox.Services.Runners;

public sealed class TerminalPythonRunner : IToolRunner
{
    private readonly ConfigService _configService;

    public TerminalPythonRunner(ConfigService configService)
    {
        _configService = configService;
    }

    public ToolType Type => ToolType.TerminalPYTHON;

    public void Run(ToolItem tool)
    {
        if (string.IsNullOrWhiteSpace(tool.Path))
            throw new InvalidOperationException("工具路径不能为空");

        if (!File.Exists(tool.Path))
            throw new FileNotFoundException($"工具文件不存在，请检查路径: {tool.Path}");

        var pythonPath = _configService.Settings.Interpreters.Python;
        if (string.IsNullOrWhiteSpace(pythonPath) || !File.Exists(pythonPath))
            throw new InvalidOperationException("请先在设置中配置 Python 路径");

        var workDir = GuiExeRunner.GetWorkDir(tool);

        var arguments = string.IsNullOrWhiteSpace(tool.Args)
            ? $"\"{tool.Path}\""
            : $"\"{tool.Path}\" {tool.Args}";

        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/K \"\"{pythonPath}\" {arguments}\"",
            WorkingDirectory = workDir,
            UseShellExecute = false
        };

        // 将 Python 解释器目录注入 PATH，方便终端中使用 python/pip 等命令
        var pythonDir = Path.GetDirectoryName(pythonPath);
        if (!string.IsNullOrEmpty(pythonDir))
        {
            psi.Environment["PATH"] = $"{pythonDir};{Environment.GetEnvironmentVariable("PATH")}";
        }

        Process.Start(psi);
    }
}
