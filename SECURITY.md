# Security Policy

## Scope

This repository contains a **local desktop shell** (WinUI 3 + WebView2) that launches a locally installed SillyTavern instance. There is no server-side deployment, no telemetry, and no bundled third-party code beyond NuGet dependencies declared in the `.csproj`.

## Supported versions

| Version | Supported |
| ------- | --------- |
| 0.1.x   | ✅        |

## Reporting a vulnerability

Please open a [GitHub Security Advisory](https://github.com/taoyu2967-dotcom/SillyTavern-WinUI/security/advisories/new) (preferred, private) or an issue titled `Security: ...` with minimal detail if advisories are unavailable.

For vulnerabilities in SillyTavern itself (the hosted web app), report upstream: https://github.com/SillyTavern/SillyTavern/blob/release/SECURITY.md

## Design constraints relevant to security review

- The Node server is always launched with a fixed executable (`node`) and a constant argument list (`server.js`); the tavern directory is validated to exist and contain `server.js` before use.
- The WebView2 user data folder is isolated under `%LOCALAPPDATA%\SillyTavernWinUI\WebView2Data`.
- Settings are stored in `%APPDATA%\SillyTavernWinUI\settings.json` and never contain secrets.
