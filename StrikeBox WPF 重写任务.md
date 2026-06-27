# StrikeBox WPF 重写任务

## 项目背景

当前 Toolkit 是一个 Python + PyQt5 + qfluentwidgets 的 Windows 桌面工具箱应用。
本地地址：C:\Users\admin\Downloads\Toolkit

现在需要用以下技术栈完整重写：

- 语言：C#
- 框架：.NET 8 / WPF
- UI 库：WPF-UI 4.3.0
- MVVM：CommunityToolkit.Mvvm 8.4.2
- 依赖注入：Microsoft.Extensions.DependencyInjection
- 配置存储：System.Text.Json（JSON 文件）
- 日志：Microsoft.Extensions.Logging 或自建简单 LogService

---

## 目标功能

重写后需要实现以下功能，与原 Python 版保持一致：

1. 工具列表展示（卡片网格布局）
2. 按分类筛选工具
3. 搜索工具（按名称模糊匹配）
4. 添加工具（弹窗表单）
5. 编辑工具（弹窗表单）
6. 删除工具（确认后删除）
7. 双击/运行工具（按类型分发）
8. 分类管理（添加、删除分类）
9. 设置页（配置解释器路径）
10. 配置持久化到本地 JSON 文件

---

## 支持的工具类型

```csharp
public enum ToolType
{
    GUIEXE,          // 普通 Windows GUI 程序
    GUIJAVA,         // Java GUI 程序（需要 java.exe）
    TerminalEXE,     // 终端运行 exe
    TerminalPYTHON,  // 终端运行 Python 脚本
    TerminalJAVA,    // 终端运行 Java 程序
    WEB              // 打开网页 URL
}
```

---

## 项目目录结构

请按以下结构创建项目：

```
StrikeBox/
  StrikeBox.sln
  StrikeBox/
    StrikeBox.csproj
    App.xaml
    App.xaml.cs
    MainWindow.xaml
    MainWindow.xaml.cs

    Models/
      ToolItem.cs
      ToolType.cs
      AppSettings.cs
      InterpreterSettings.cs

    ViewModels/
      MainWindowViewModel.cs
      ToolsViewModel.cs
      SettingsViewModel.cs
      CategoryManagerViewModel.cs
      EditToolDialogViewModel.cs

    Views/
      ToolsView.xaml
      ToolsView.xaml.cs
      SettingsView.xaml
      SettingsView.xaml.cs
      CategoryManagerView.xaml
      CategoryManagerView.xaml.cs

    Views/Dialogs/
      EditToolDialog.xaml
      EditToolDialog.xaml.cs

    Services/
      ConfigService.cs
      ToolExecutionService.cs
      NavigationService.cs
      LogService.cs

    Services/Runners/
      IToolRunner.cs
      GuiExeRunner.cs
      GuiJavaRunner.cs
      TerminalExeRunner.cs
      TerminalPythonRunner.cs
      TerminalJavaRunner.cs
      WebRunner.cs

    Styles/
      Styles.xaml
      Cards.xaml

    Converters/
      BoolToVisibilityConverter.cs
      InverseBoolToVisibilityConverter.cs
      ToolTypeToStringConverter.cs

    Assets/
      app-icon.ico
```

---

## NuGet 依赖

在 `StrikeBox.csproj` 中添加以下依赖：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
    <ApplicationIcon>Assets\app-icon.ico</ApplicationIcon>
    <Version>1.0.0</Version>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="WPF-UI" Version="4.3.0" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.2" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
  </ItemGroup>
</Project>
```

---

## 数据模型

### Models/ToolType.cs

```csharp
namespace StrikeBox.Models;

public enum ToolType
{
    GUIEXE,
    GUIJAVA,
    TerminalEXE,
    TerminalPYTHON,
    TerminalJAVA,
    WEB
}
```

### Models/ToolItem.cs

```csharp
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
}
```

### Models/InterpreterSettings.cs

```csharp
namespace StrikeBox.Models;

public sealed class InterpreterSettings
{
    public string Python { get; set; } = string.Empty;
    public string Java8 { get; set; } = string.Empty;
    public string Java11 { get; set; } = string.Empty;
    public string Java17 { get; set; } = string.Empty;
}
```

### Models/AppSettings.cs

```csharp
namespace StrikeBox.Models;

public sealed class AppSettings
{
    public InterpreterSettings Interpreters { get; set; } = new();
    public List<ToolItem> Tools { get; set; } = new();
    public List<string> Categories { get; set; } = new() { "默认分类" };
}
```

---

## 服务层

### Services/ConfigService.cs

负责读写 `%AppData%/StrikeBox/config.json`，实现工具和分类的增删改查。

需要实现以下方法：

```csharp
public sealed class ConfigService
{
    public AppSettings Settings { get; private set; }

    public void Load();
    public void Save();

    public IReadOnlyList<ToolItem> GetTools();
    public IReadOnlyList<string> GetCategories();

    public void AddTool(ToolItem tool);
    public void UpdateTool(ToolItem tool);
    public void RemoveTool(string toolId);

    public void AddCategory(string category);
    public void RemoveCategory(string category);
    public void EnsureCategory(string category);
}
```

### Services/Runners/IToolRunner.cs

```csharp
using StrikeBox.Models;

namespace StrikeBox.Services.Runners;

public interface IToolRunner
{
    ToolType Type { get; }
    void Run(ToolItem tool);
}
```

### Services/Runners 各 Runner 实现

每种工具类型一个 Runner，实现 `IToolRunner`：

- `GuiExeRunner`：检查路径存在，`Process.Start` 启动 exe
- `GuiJavaRunner`：使用 `InterpreterSettings.Java*` 对应版本启动 jar
- `TerminalExeRunner`：在 cmd 或 Windows Terminal 中运行 exe
- `TerminalPythonRunner`：使用配置的 Python 路径在终端运行脚本
- `TerminalJavaRunner`：在终端运行 Java 程序
- `WebRunner`：自动补全 https://，用 `Process.Start` 打开浏览器

所有 Runner 运行前需要：
1. 检查路径是否为空
2. 非 WEB 类型检查文件是否存在
3. WorkDir 为空时自动使用文件所在目录
4. 失败时抛出含中文提示的异常

### Services/ToolExecutionService.cs

```csharp
public sealed class ToolExecutionService
{
    // 通过 DI 注入所有 IToolRunner
    // 根据 tool.Type 选择对应 Runner
    // 运行失败时捕获异常并通知 UI
}
```

---

## 依赖注入配置

### App.xaml.cs

在 `OnStartup` 中配置 DI 容器：

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    var services = new ServiceCollection();

    // Services
    services.AddSingleton<ConfigService>();
    services.AddSingleton<ToolExecutionService>();
    services.AddSingleton<NavigationService>();
    services.AddSingleton<LogService>();

    // Runners（全部注册为 IToolRunner）
    services.AddSingleton<IToolRunner, GuiExeRunner>();
    services.AddSingleton<IToolRunner, GuiJavaRunner>();
    services.AddSingleton<IToolRunner, TerminalExeRunner>();
    services.AddSingleton<IToolRunner, TerminalPythonRunner>();
    services.AddSingleton<IToolRunner, TerminalJavaRunner>();
    services.AddSingleton<IToolRunner, WebRunner>();

    // ViewModels
    services.AddSingleton<MainWindowViewModel>();
    services.AddSingleton<ToolsViewModel>();
    services.AddSingleton<SettingsViewModel>();
    services.AddTransient<CategoryManagerViewModel>();
    services.AddTransient<EditToolDialogViewModel>();

    // Windows
    services.AddSingleton<MainWindow>();

    Services = services.BuildServiceProvider();

    var mainWindow = Services.GetRequiredService<MainWindow>();
    mainWindow.Show();

    base.OnStartup(e);
}
```

---

## UI 要求

### App.xaml 主题配置

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ui:ThemesDictionary Theme="Dark" />
            <ui:ControlsDictionary />
            <ResourceDictionary Source="Styles/Styles.xaml"/>
            <ResourceDictionary Source="Styles/Cards.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 主窗口布局（MainWindow.xaml）

- 使用 `ui:FluentWindow`
- 左侧 240px 导航栏，包含分类列表
- 右侧内容区
- 顶部有搜索框（宽 300）和"添加工具"按钮
- 中间用 `ScrollViewer` + `WrapPanel` 展示工具卡片网格

### 工具卡片样式

每个工具卡片：
- 宽 180，高 100
- 圆角 10
- 深色背景 `#2B2B2B`
- 显示工具名称（16px SemiBold）
- 显示工具类型（12px 半透明）
- 双击触发运行命令
- 右键菜单：编辑、删除

### 添加/编辑工具弹窗（EditToolDialog）

表单字段：
- 工具名称（必填，TextBox）
- 分类（EditableComboBox，可输入新分类）
- 工具类型（ComboBox，枚举 ToolType）
- 路径（TextBox + 浏览按钮）
- 工作目录（TextBox + 浏览按钮，可选）
- 启动参数（TextBox，可选）
- Java 版本（ComboBox，仅 GUIJAVA / TerminalJAVA 时显示）

### 分类管理页（CategoryManagerView）

- 显示当前所有分类列表
- 每行有删除按钮
- 顶部有新增分类输入框和添加按钮
- 删除分类时，该分类下的工具自动移动到"默认分类"

### 设置页（SettingsView）

- Python 解释器路径（TextBox + 浏览按钮）
- Java 8 路径（TextBox + 浏览按钮）
- Java 11 路径（TextBox + 浏览按钮）
- Java 17 路径（TextBox + 浏览按钮）
- 保存按钮
- 路径使用 OpenFileDialog 选择 exe 文件

---

## ViewModel 要求

所有 ViewModel 继承 `ObservableObject`，使用 `[ObservableProperty]` 和 `[RelayCommand]`。

### ToolsViewModel 需要以下属性和命令

```csharp
// 属性
ObservableCollection<ToolItem> Tools
ObservableCollection<string> Categories
string SearchText         // 双向绑定，变化时自动过滤
string SelectedCategory   // 选中分类

// 命令
RunToolCommand(ToolItem tool)
AddToolCommand()
EditToolCommand(ToolItem tool)
DeleteToolCommand(ToolItem tool)
RefreshCommand()
```

搜索时按名称模糊匹配，分类筛选支持"全部"选项。

### EditToolDialogViewModel

接收一个可选的 `ToolItem`（null 表示新增，有值表示编辑），
返回编辑后的 `ToolItem`，由调用方负责调用 `ConfigService` 保存。

---

## 错误处理要求

1. 运行工具失败时，显示 `ui:MessageBox` 弹窗，中文提示错误原因
2. 路径不存在时提示"工具文件不存在，请检查路径"
3. 解释器未配置时提示"请先在设置中配置 Python/Java 路径"
4. 添加工具名称或路径为空时，提示"工具名称和路径不能为空"
5. 所有异常写入 LogService 日志

---

## 配置文件路径

配置文件保存到：
```
%AppData%\StrikeBox\config.json
```

JSON 结构示例：

```json
{
  "Interpreters": {
    "Python": "C:\\Python\\python.exe",
    "Java8": "",
    "Java11": "",
    "Java17": "C:\\Program Files\\Java\\jdk-17\\bin\\java.exe"
  },
  "Tools": [
    {
      "Id": "uuid-xxxx",
      "Name": "记事本",
      "Category": "系统工具",
      "Type": "GUIEXE",
      "Path": "C:\\Windows\\System32\\notepad.exe",
      "WorkDir": "",
      "Args": "",
      "JavaVersion": null
    }
  ],
  "Categories": ["默认分类", "系统工具", "开发工具"]
}
```

---

## 实现顺序建议

请按以下顺序实现，每步完成后确保可编译：

1. 创建项目结构，配置 `.csproj`，安装 NuGet 包
2. 实现 Models（ToolItem、ToolType、AppSettings、InterpreterSettings）
3. 实现 ConfigService（JSON 读写）
4. 实现 IToolRunner 接口和所有 Runner
5. 实现 ToolExecutionService
6. 配置 App.xaml 主题和 DI 容器（App.xaml.cs）
7. 实现 MainWindow.xaml 基础布局
8. 实现 ToolsViewModel 和 ToolsView.xaml
9. 实现 EditToolDialogViewModel 和 EditToolDialog.xaml
10. 实现 CategoryManagerViewModel 和 CategoryManagerView.xaml
11. 实现 SettingsViewModel 和 SettingsView.xaml
12. 实现导航页面切换逻辑
13. 完善错误处理和提示
14. 整体测试：添加工具、运行工具、分类管理、设置保存

---

## 注意事项

- 所有 UI 文字使用中文
- 默认深色主题
- 关闭按钮直接退出应用（暂不做托盘常驻）
- ViewModel 不直接引用 View，不直接操作 UI 控件
- 所有 View 通过 DataContext 绑定对应 ViewModel
- 严格使用 MVVM，不在 code-behind 写业务逻辑
- 兼容 Windows 10 / Windows 11