namespace StrikeBox.Models;

public sealed class InterpreterSettings
{
    /// <summary>
    /// Python 解释器列表，用户可自定义版本名和路径
    /// </summary>
    public List<InterpreterEntry> PythonInterpreters { get; set; } = new();

    /// <summary>
    /// Java 解释器列表，用户可自定义版本名和路径
    /// </summary>
    public List<InterpreterEntry> JavaInterpreters { get; set; } = new();

    /// <summary>
    /// 根据版本标识获取 Python 路径。
    /// 未指定版本时返回列表中第一个可用路径。
    /// </summary>
    public string? GetPythonPath(string? version)
    {
        if (!string.IsNullOrWhiteSpace(version))
        {
            var entry = PythonInterpreters.FirstOrDefault(
                p => p.Version.Equals(version, StringComparison.OrdinalIgnoreCase));
            if (entry != null && !string.IsNullOrWhiteSpace(entry.Path))
                return entry.Path;
        }
        return PythonInterpreters.FirstOrDefault()?.Path;
    }

    /// <summary>
    /// 根据版本标识获取 Java 路径。
    /// 未指定版本时返回列表中第一个可用路径。
    /// </summary>
    public string? GetJavaPath(string? version)
    {
        if (!string.IsNullOrWhiteSpace(version))
        {
            var entry = JavaInterpreters.FirstOrDefault(
                j => j.Version.Equals(version, StringComparison.OrdinalIgnoreCase));
            if (entry != null && !string.IsNullOrWhiteSpace(entry.Path))
                return entry.Path;
        }
        return JavaInterpreters.FirstOrDefault()?.Path;
    }
}
