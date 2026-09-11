# Contributing

Thanks for your interest in improving **SillyTavern-WinUI**!

## How to help

- **Bug reports**: open an issue with your Windows version, GPU, .NET SDK version, and the steps to reproduce (console output helps too).
- **Feature requests**: open an issue starting with `Feature:` in the title.
- **Pull requests**: fork → branch (`feat/...` or `fix/...`) → PR. Keep changes minimal and match the existing code style.

## Ground rules

1. **Zero UI drift**: the shell must not alter SillyTavern's own web frontend. If your change requires touching files under SillyTavern's `public/` directory, it belongs upstream in SillyTavern, not here.
2. **Honest features only**: no fake toggles (e.g. pretending to enable DLSS/FSR in a WebView2 app). Driver-level technologies can only be surfaced as guidance/entry points — see the README.
3. **No process-injection surface**: the server is always launched with a fixed executable (`node`) and argument list (`server.js`); don't introduce shell string interpolation.
4. **Build before PR**: `dotnet build -p:Platform=x64` must pass with 0 warnings.

## Development

```powershell
cd src\SillyTavernWinUI
dotnet build -p:Platform=x64
dotnet run -p:Platform=x64
```

See [README.md](README.md) / [README-zh-cn.md](README-zh-cn.md) for architecture and the project layout.
