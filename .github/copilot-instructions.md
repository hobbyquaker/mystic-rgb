# mystic-rgb — Copilot Instructions

## Project Overview

`mystic-rgb` is a Windows CLI tool to control RGB LEDs on MSI hardware via the official MysticLight SDK (`MysticLight_SDK.dll`). It runs as a `WinExe` (no console window) and uses a Windows Scheduled Task for UAC-free elevation.

## Architecture

```
mystic-rgb.exe --effect <name> [--color <value>]
    │
    ├── if already elevated → RunDeviceList() directly
    │
    └── if not elevated → writes action to temp file
                        → triggers Scheduled Task
                        → Task runs mystic-rgb.exe --task-mode (elevated)
                        → output written to temp file
                        → main process reads result
```

### Key components

- **`mystic-rgb/Program.cs`** — main entry point (top-level statements)
  - `--setup`: registers the Windows Scheduled Task (must run as admin, done by installer)
  - `--task-mode`: runs elevated via the Scheduled Task, writes output to temp file
  - `--effect <name> [--color <value>]`: sets LED effect/color
- **`mystic-rgb/installer/Program.cs`** — WinForms installer
  - Copies files to `%ProgramFiles%\mystic-rgb`
  - Runs `mystic-rgb.exe --setup` to register the Scheduled Task
- **`mystic-rgb/Mystic_light_SDK_1.0.0.08/MysticLight_SDK_x64.dll`** — native SDK (x64 only)

## Tech Stack

- .NET 10, C#, Windows only (`net10.0-windows`)
- `OutputType`: `WinExe` — no console window ever appears
- `PlatformTarget`: `x64` (SDK is x64-only)
- P/Invoke for `MysticLight_SDK.dll` and `kernel32.dll`
- Windows Scheduled Task for privilege elevation without UAC prompts

## Coding Conventions

- Top-level statements (no explicit `class Program`)
- No `Console.Write*` in the main executable (it's a `WinExe`)
- Errors are silently swallowed or written to temp files (no UI)
- Retry loops with `MaxRetries` / `RetryDelayMs` constants for SDK calls
- LED effect styles are cached in a temp file for reuse

## Temp File Protocol

| File | Purpose |
|---|---|
| `%TEMP%\mystic-rgb-action.txt` | Effect + color passed to the Task |
| `%TEMP%\mystic-rgb-output.txt` | Output written by `--task-mode` |
| `%TEMP%\mystic-rgb-done.flag` | Signals task completion |
| `%TEMP%\mystic-rgb-styles-cache.txt` | Cached LED style names |

## Release / CI

- GitHub Actions workflow at `.github/workflows/release.yml`
- Triggered by pushing a version tag (`v*`)
- Publishes `mystic-rgb.exe` and `mystic-rgb-installer.exe` as self-contained single-file binaries for `win-x64`
- `MysticLight_SDK.dll` is included as a separate download
