using StrikeBox.Models;
using System.Diagnostics;
using System.IO;

namespace StrikeBox.Services.Runners;

public sealed class GuiExeRunner : IToolRunner
{
    public ToolType Type => ToolType.GUIEXE;

    public void Run(ToolItem tool)
    {
        if (string.IsNullOrWhiteSpace(tool.Path))
            throw new InvalidOperationException("工具路径不能为空");

        if (!File.Exists(tool.Path))
            throw new FileNotFoundException($"工具文件不存在，请检查路径: {tool.Path}");

        var workDir = GetWorkDir(tool);

        var psi = new ProcessStartInfo
        {
            FileName = tool.Path,
            Arguments = tool.Args ?? string.Empty,
            WorkingDirectory = workDir,
            UseShellExecute = true
        };

        Process.Start(psi);
    }

    public static string GetWorkDir(ToolItem tool)
    {
        if (!string.IsNullOrWhiteSpace(tool.WorkDir) && Directory.Exists(tool.WorkDir))
            return tool.WorkDir;

        var fileDir = Path.GetDirectoryName(tool.Path);
        return !string.IsNullOrWhiteSpace(fileDir) && Directory.Exists(fileDir)
            ? fileDir
            : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
}
