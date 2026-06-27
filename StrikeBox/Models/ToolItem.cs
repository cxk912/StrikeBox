namespace StrikeBox.Models;

public sealed class ToolItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "默认分类";
    public ToolType Type { get; set; } = ToolType.GUIEXE;
    public string Path { get; set; } = string.Empty;
    public string WorkDir { get; set; } = string.Empty;
    public string Args { get; set; } = string.Empty;
    public string? JavaVersion { get; set; }
    public string? PythonVersion { get; set; }
}
