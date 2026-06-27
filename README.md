# StrikeBox - 工具箱

一站式安全工具启动面板，统一管理各类安全测试工具，分类浏览、快速搜索、一键启动。

## 功能

- **工具管理** — 添加/编辑/删除工具，自定义分类，支持搜索筛选
- **多类型启动** — GUI EXE、GUI Java、终端 EXE、终端 Python、终端 Java、网页链接
- **解释器配置** — 集中管理 Python、Java 8/11/17 运行环境路径
- **深色/浅色主题** — 一键切换，Mica 背景材质
- **数据持久化** — JSON 配置文件，自动保存在 AppData

## 技术栈

| 组件 | 说明 |
|------|------|
| .NET 8 | WPF 桌面应用 |
| [WPF-UI](https://github.com/lepoco/wpfui) | Fluent Design 现代化 UI |
| [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) | MVVM 架构框架 |
| Microsoft.Extensions.DI | 依赖注入 |

## 项目结构

```
StrikeBox/
├── Models/          # 数据模型
├── ViewModels/      # 视图模型（MVVM）
├── Views/           # 视图 + 对话框
├── Services/        # 配置、日志、导航、工具执行
│   └── Runners/     # 六种工具启动器
├── Converters/      # WPF 值转换器
├── Styles/          # 全局样式 & 卡片样式
└── Assets/          # 资源文件
```

## 构建

```bash
dotnet build
dotnet run --project StrikeBox/StrikeBox.csproj
```

或使用 Visual Studio 2022+ 打开 `StrikeBox.sln`。
