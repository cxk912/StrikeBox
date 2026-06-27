namespace StrikeBox.Models;

/// <summary>
/// 解释器条目：版本名 + 可执行文件路径
/// </summary>
public sealed class InterpreterEntry
{
    public string Version { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}
