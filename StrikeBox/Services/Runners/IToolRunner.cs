using StrikeBox.Models;

namespace StrikeBox.Services.Runners;

public interface IToolRunner
{
    ToolType Type { get; }
    void Run(ToolItem tool);
}
