using StrikeBox.Models;
using System.Diagnostics;
using System.IO;

namespace StrikeBox.Services.Runners;

public sealed class TerminalJavaRunner : IToolRunner
{
    private readonly ConfigService _configService;

    public TerminalJavaRunner(ConfigService configService)
    {
        _configService = configService;
    }

    public ToolType Type => ToolType.TerminalJAVA;

    public void Run(ToolItem tool)
    {
        if (string.IsNullOrWhiteSpace(tool.Path))
            throw new InvalidOperationException("工具路径不能为空");

        if (!File.Exists(tool.Path))
            throw new FileNotFoundException($"工具文件不存在，请检查路径: {tool.Path}");

        var interpreters = _configService.Settings.Interpreters;
        var javaPath = interpreters.GetJavaPath(tool.JavaVersion);
        if (string.IsNullOrWhiteSpace(javaPath) || !File.Exists(javaPath))
            throw new InvalidOperationException("请先在设置中配置 Java 路径");

        var workDir = GuiExeRunner.GetWorkDir(tool);

        var arguments = string.IsNullOrWhiteSpace(tool.Args)
            ? $"-jar \"{tool.Path}\""
            : $"-jar \"{tool.Path}\" {tool.Args}";

        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/K \"\"{javaPath}\" {arguments}\"",
            WorkingDirectory = workDir,
            UseShellExecute = false
        };

        // 将 Java bin 目录注入 PATH，方便终端中使用 java/javac/jar 等命令
        var javaDir = Path.GetDirectoryName(javaPath);
        if (!string.IsNullOrEmpty(javaDir))
        {
            psi.Environment["PATH"] = $"{javaDir};{Environment.GetEnvironmentVariable("PATH")}";
        }

        Process.Start(psi);
    }
}
