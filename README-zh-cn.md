<a name="readme-top"></a>

<div align="center">

<h1 align="center">SillyTavern WinUI</h1>

English | <a href="README-zh-cn.md">简体中文</a>

</div>

SillyTavern（酒馆）的 WinUI 3 桌面前端外壳。

- **与原版零差异**：内嵌 WebView2 直接加载酒馆自身的 Web 界面（`http://127.0.0.1:<port>`），不改一行酒馆前端代码，角色卡、聊天、扩展、主题全部原样。
- **服务端生命周期管理**：启动时自动拉起 `node server.js`（或附加到已运行实例），关闭时按设置退出或最小化到托盘；由外壳拉起的进程在退出时整树结束。
- **设置**：酒馆目录、端口覆盖、自动启动、关闭行为、启动最大化。配置文件位于 `%APPDATA%\SillyTavernWinUI\settings.json`，WebView2 用户数据（酒馆 localStorage）位于 `%LOCALAPPDATA%\SillyTavernWinUI\WebView2Data`。

## 关于"超分 / 帧生成"（DLSS / FSR / XeSS / AFMF）

设置页提供两类**真实有效**的选项，不做假开关：

1. **界面缩放（80%–120%）**：等效浏览器 Ctrl+滚轮的页面缩放，低于 100% 时页面以更少像素渲染，滚动与动画更流畅。
2. **驱动层入口**：自动检测 GPU 厂商，一键打开 NVIDIA App / AMD Software / Intel 显卡控制中心。

需要说明：DLSS、FSR、XeSS、AFMF、Intel Smooth Motion 是显卡驱动或游戏引擎层的技术，只作用于受支持的 3D 游戏，**任何桌面/网页应用都无法自行集成或开启**，本应用也不例外。

## 构建

要求：Windows 10 1809+ / .NET SDK 8+ / Node.js（PATH 中的 `node`）。

```powershell
cd src\SillyTavernWinUI
dotnet build -p:Platform=x64
# 运行：
dotnet run -p:Platform=x64
# 或直接运行 bin\x64\Debug\net8.0-windows10.0.19041.0\SillyTavernWinUI.exe
```

首次构建会通过 NuGet 还原 Windows App SDK（自包含模式，输出体积较大）。

## 项目结构

```
src/SillyTavernWinUI/
├── App.xaml(.cs)              # 应用入口，全局设置
├── MainWindow.xaml(.cs)       # 主窗口：WebView2 + 启动遮罩 + 托盘
├── SettingsWindow.xaml(.cs)   # 设置窗口（服务端 / 超分 / 显卡）
└── Services/
    ├── AppSettings.cs         # settings.json 读写与端口解析
    ├── ServerManager.cs       # 探测 / 等待就绪 / 结束进程树
    ├── ServerProcessLauncher.cs # 固定命令启动 node server.js
    ├── GpuDetector.cs         # WMI 显卡枚举与厂商识别
    ├── VendorPanelLauncher.cs # 打开厂商控制面板
    ├── RelayCommand.cs        # 托盘双击命令
    └── Native.cs              # Win32 互操作
```

## License

外壳代码：MIT。SillyTavern 本体遵循其自身许可证，本项目不包含酒馆代码。
