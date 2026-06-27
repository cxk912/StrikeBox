using StrikeBox.Models;
using System.Diagnostics;
using System.IO;

namespace StrikeBox.Services.Runners;

public sealed class TerminalExeRunner : IToolRunner
{
    public ToolType Type => ToolType.TerminalEXE;

    public void Run(ToolItem tool)
    {
        if (string.IsNullOrWhiteSpace(tool.Path))
            throw new InvalidOperationException("工具路径不能为空");

        if (!File.Exists(tool.Path))
            throw new FileNotFoundException($"工具文件不存在，请检查路径: {tool.Path}");

        var workDir = GuiExeRunner.GetWorkDir(tool);

        var arguments = string.IsNullOrWhiteSpace(tool.Args)
            ? string.Empty
            : $" {tool.Args}";

        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/K \"\"{tool.Path}\"{arguments}\"",
            WorkingDirectory = workDir,
            UseShellExecute = false
        };

        // 将工具所在目录注入 PATH，方便终端中访问依赖
        var toolDir = Path.GetDirectoryName(tool.Path);
        if (!string.IsNullOrEmpty(toolDir))
        {
            psi.Environment["PATH"] = $"{toolDir};{Environment.GetEnvironmentVariable("PATH")}";
        }

        Process.Start(psi);
    }
}
