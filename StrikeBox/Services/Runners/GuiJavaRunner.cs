using StrikeBox.Models;
using System.Diagnostics;
using System.IO;

namespace StrikeBox.Services.Runners;

public sealed class GuiJavaRunner : IToolRunner
{
    private readonly ConfigService _configService;

    public GuiJavaRunner(ConfigService configService)
    {
        _configService = configService;
    }

    public ToolType Type => ToolType.GUIJAVA;

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

        // GUI 程序优先使用 javaw.exe 避免弹出 cmd 黑窗口
        var javaDir = Path.GetDirectoryName(javaPath);
        if (!string.IsNullOrEmpty(javaDir))
        {
            var javawPath = Path.Combine(javaDir, "javaw.exe");
            if (File.Exists(javawPath))
            {
                javaPath = javawPath;
            }
        }

        var workDir = GuiExeRunner.GetWorkDir(tool);

        var psi = new ProcessStartInfo
        {
            FileName = javaPath,
            Arguments = $"-jar \"{tool.Path}\" {tool.Args}",
            WorkingDirectory = workDir,
            UseShellExecute = true
        };

        Process.Start(psi);
    }
}
