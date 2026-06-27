namespace StrikeBox.Models;

public sealed class AppSettings
{
    public InterpreterSettings Interpreters { get; set; } = new();
    public List<ToolItem> Tools { get; set; } = new();
    public List<string> Categories { get; set; } = new() { "默认分类" };
}
