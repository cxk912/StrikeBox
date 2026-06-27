namespace StrikeBox.Models;

public sealed class InterpreterSettings
{
    public string Python { get; set; } = string.Empty;
    public string Java8 { get; set; } = string.Empty;
    public string Java11 { get; set; } = string.Empty;
    public string Java17 { get; set; } = string.Empty;

    /// <summary>
    /// 根据版本标识获取 Java 路径。
    /// 未指定版本时自动选择：Java17 > Java11 > Java8。
    /// </summary>
    public string GetJavaPath(string? version)
    {
        return version switch
        {
            "Java8" => Java8,
            "Java11" => Java11,
            "Java17" => Java17,
            _ => !string.IsNullOrWhiteSpace(Java17) ? Java17 :
                 !string.IsNullOrWhiteSpace(Java11) ? Java11 :
                 Java8
        };
    }
}
