<a name="readme-top"></a>

<div align="center">

<h1 align="center">SillyTavern WinUI</h1>

English | <a href="README-zh-cn.md">简体中文</a>

</div>

A WinUI 3 desktop shell for SillyTavern.

- **Zero difference from the original**: an embedded WebView2 loads SillyTavern's own web UI (`http://127.0.0.1:<port>`) without touching a single line of its frontend — character cards, chats, extensions and themes all behave exactly as in the browser.
- **Server lifecycle management**: automatically launches `node server.js` on startup (or attaches to an already-running instance); on exit it either quits or minimizes to the tray per your settings, killing the whole process tree it spawned.
- **Settings**: tavern folder, port override, auto-start, close behavior, launch maximized. Settings live in `%APPDATA%\SillyTavernWinUI\settings.json`; the WebView2 user data (tavern localStorage) lives in `%LOCALAPPDATA%\SillyTavernWinUI\WebView2Data`.

## About "super resolution / frame generation" (DLSS / FSR / XeSS / AFMF)

The settings page offers two kinds of options that are **real and effective** — no fake toggles:

1. **UI scale (80%–120%)**: the same as browser Ctrl+wheel page zoom; below 100% the page renders fewer pixels, making scrolling and animations smoother.
2. **Driver-level entry points**: detects your GPU vendor and opens NVIDIA App / AMD Software / Intel Graphics Control Center with one click.

Please note: DLSS, FSR, XeSS, AFMF and Intel Smooth Motion are driver- or game-engine-level technologies that only apply to supported 3D games. **No desktop or web app — including this one — can integrate or enable them by itself.**

## Build

Requirements: Windows 10 1809+ / .NET SDK 8+ / Node.js (`node` on PATH).

```powershell
cd src\SillyTavernWinUI
dotnet build -p:Platform=x64
# Run:
dotnet run -p:Platform=x64
# or launch bin\x64\Debug\net8.0-windows10.0.19041.0\SillyTavernWinUI.exe directly
```

The first build restores the Windows App SDK via NuGet (self-contained mode, so the output is fairly large).

## Project layout

```
src/SillyTavernWinUI/
├── App.xaml(.cs)              # App entry, global settings
├── MainWindow.xaml(.cs)       # Main window: WebView2 + startup overlay + tray
├── SettingsWindow.xaml(.cs)   # Settings window (server / UI scale / GPU)
└── Services/
    ├── AppSettings.cs         # settings.json persistence and port resolution
    ├── ServerManager.cs       # probe / wait-ready / kill process tree
    ├── ServerProcessLauncher.cs # launches `node server.js` with a fixed command
    ├── GpuDetector.cs         # WMI GPU enumeration and vendor detection
    ├── VendorPanelLauncher.cs # opens vendor control panels
    ├── RelayCommand.cs        # tray double-click command
    └── Native.cs              # Win32 interop
```

## License

Shell code: MIT. SillyTavern itself is licensed under its own license; this project does not include any SillyTavern code.
