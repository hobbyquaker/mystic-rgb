# mystic-rgb

Command-line tool to control MSI Mystic Light effects/colors via `MysticLight_SDK.dll`.

> This tool is **100% vibe-coded with Claude Sonnet 4.6**.

## Requirements

- Windows
- MSI Center / Mystic Light installed and running
- `MysticLight_SDK.dll` available in output directory (handled by project build)

## Setup (once)

Run once as administrator to register scheduled task for UAC-free daily usage:

```powershell
mystic-rgb.exe --setup
```

## Usage

```powershell
mystic-rgb.exe --effect <effectName> [--color <color>]
```

Examples:

```powershell
mystic-rgb.exe --effect Off
mystic-rgb.exe --effect Steady --color blue
mystic-rgb.exe --effect Steady --color #0000FF
mystic-rgb.exe --effect Meteor --color f80
```

## Color Formats

- Named: `red`, `green`, `blue`, `cyan`, `magenta`, `yellow`, `white`
- Hex: `#RGB`, `RGB`, `#RRGGBB`, `RRGGBB`

## License

MIT — see `LICENSE`.
