using StrikeBox.Models;
using System.IO;
using System.Text.Json;

namespace StrikeBox.Services;

public sealed class ConfigService
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "StrikeBox");

    private static readonly string ConfigFile = Path.Combine(ConfigDir, "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public AppSettings Settings { get; private set; } = new();

    private readonly LogService _logService;

    public ConfigService(LogService logService)
    {
        _logService = logService;
    }

    public void Load()
    {
        try
        {
            if (!Directory.Exists(ConfigDir))
                Directory.CreateDirectory(ConfigDir);

            if (File.Exists(ConfigFile))
            {
                var json = File.ReadAllText(ConfigFile);
                Settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();

                // 确保默认分类始终存在（反序列化可能覆盖构造函数默认值）
                if (!Settings.Categories.Contains("默认分类"))
                    Settings.Categories.Insert(0, "默认分类");

                _logService.Info("配置文件加载成功");
            }
            else
            {
                Settings = new AppSettings();
                Save();
                _logService.Info("创建默认配置文件");
            }
        }
        catch (Exception ex)
        {
            _logService.Error($"加载配置文件失败: {ex.Message}");
            Settings = new AppSettings();
        }
    }

    public void Save()
    {
        try
        {
            if (!Directory.Exists(ConfigDir))
                Directory.CreateDirectory(ConfigDir);

            var json = JsonSerializer.Serialize(Settings, JsonOptions);
            File.WriteAllText(ConfigFile, json);
            _logService.Info("配置文件保存成功");
        }
        catch (Exception ex)
        {
            _logService.Error($"保存配置文件失败: {ex.Message}");
            throw;
        }
    }

    public IReadOnlyList<ToolItem> GetTools() => Settings.Tools.AsReadOnly();

    public IReadOnlyList<string> GetCategories() => Settings.Categories.AsReadOnly();

    public void AddTool(ToolItem tool)
    {
        ArgumentNullException.ThrowIfNull(tool);
        Settings.Tools.Add(tool);
        EnsureCategory(tool.Category);
        Save();
        _logService.Info($"添加工具: {tool.Name}");
    }

    public void UpdateTool(ToolItem tool)
    {
        ArgumentNullException.ThrowIfNull(tool);
        var existing = Settings.Tools.FirstOrDefault(t => t.Id == tool.Id);
        if (existing != null)
        {
            var index = Settings.Tools.IndexOf(existing);
            Settings.Tools[index] = tool;
            EnsureCategory(tool.Category);
            Save();
            _logService.Info($"更新工具: {tool.Name}");
        }
    }

    public void RemoveTool(string toolId)
    {
        var tool = Settings.Tools.FirstOrDefault(t => t.Id == toolId);
        if (tool != null)
        {
            Settings.Tools.Remove(tool);
            Save();
            _logService.Info($"删除工具: {tool.Name}");
        }
    }

    public void AddCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return;

        if (!Settings.Categories.Contains(category))
        {
            Settings.Categories.Add(category);
            Save();
            _logService.Info($"添加分类: {category}");
        }
    }

    public void RemoveCategory(string category)
    {
        if (category == "默认分类")
            return;

        if (Settings.Categories.Remove(category))
        {
            // 将删除分类下的工具移动到默认分类
            foreach (var tool in Settings.Tools.Where(t => t.Category == category))
            {
                tool.Category = "默认分类";
            }
            Save();
            _logService.Info($"删除分类: {category}");
        }
    }

    private void EnsureCategory(string category)
    {
        if (!string.IsNullOrWhiteSpace(category) && !Settings.Categories.Contains(category))
        {
            Settings.Categories.Add(category);
        }
    }
}
