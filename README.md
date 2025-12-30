# TNL DPS Meter

DPS Meter for Throne and Liberty game, written in .NET WPF.

**Latest Version: v2.3** - [Download](https://github.com/Gulliby/tnl-dps-meter/releases/tag/v2.3)

**Main Branch: main** (master merged and deleted)

## Features

- Window always on top of other windows
- No standard window title bar - clean interface
- Window dragging from any area
- Compact size (266x130px) and high transparency (30% opacity) for minimal distraction
- Real-time damage tracking with combat time display
- Shortened display of large numbers (8905 → 8.9k, 3966236 → 3966.2k)
- Display mode selection: Last Combat, Overall Damage, and combat history
- Close button (×) in the top right corner
- Auto-hide interface - tab headers and footer hide when mouse leaves, DPS data remains visible
- Flash effect - window flashes purple when new combat data appears
- Dropdown list - mouse hover shows menu for display mode selection
- Current log file name display in the bottom
- Combat history with automatic session saving
- Log reading from `%LOCALAPPDATA%\TL\SAVED\COMBATLOGS` folder
- Extended CSV parsing with critical hits, heavy attacks, and target names
- Overall Damage: shows statistics for the entire file from first to last record, excluding pauses > 10 seconds
- Last Combat: shows statistics for the last set of new data in the log file, excluding pauses > 10 seconds
- Combat History: saved sessions with target names and automatic naming

## Installation and Launch

1. Ensure you have .NET 7.0 or higher installed
2. Clone the repository
3. Run the application using one of the methods:

   **Method 1: Via command line**
   ```bash
   cd TNL_DPS_Meter
   dotnet run
   ```

   **Method 2: Via .bat file (Windows)**
   ```bash
   run_dps_meter.bat
   ```

## Log Format

The application reads files from the `%LOCALAPPDATA%\TL\SAVED\COMBATLOGS\` folder.

### Supported Format:

**Throne and Liberty CSV Format (CombatLogVersion,4)**
```
CombatLogVersion,4
{Date},DamageDone,{AbilityName},{ServerTick},{DamageDoneByAbilityHit},{isCrit},{isHeavy},{calculationDescriptor},{playerName},{targetName}
```

Example:
```
CombatLogVersion,4
20251229-01:37:18:962,DamageDone,Basic Shot,947581240,646,1,0,kMaxDamageByCriticalDecision,gulli,Practice Dummy
20251229-01:37:19:158,DamageDone,Basic Shot,947515856,978,0,1,kNormalHit,gulli,Practice Dummy
```

**Complete field format:**
- `{Date}`: YYYYMMDD-HH:MM:SS:MS (timestamp)
- `DamageDone`: constant (event type)
- `{AbilityName}`: ability name
- `{ServerTick}`: internal server tick
- `{DamageDoneByAbilityHit}`: **DAMAGE AMOUNT** (key parameter for calculations)
- `{isCrit}`: 0/1 (0 = normal hit, 1 = critical hit)
- `{isHeavy}`: 0/1 (0 = normal attack, 1 = heavy attack)
- `{calculationDescriptor}`: hit type (kNormalHit, kMaxDamageByCriticalDecision, kMiss)
- `{playerName}`: player name
- `{targetName}`: target name

**All fields are parsed and used for combat analysis and history tracking**

### File Names:
- Expected format: `TLCombatLog-YYYYMMDD_HHMMSS.txt`
- Example: `TLCombatLog-251229_013714.txt`

## Controls

- Window dragging from any area
- Close button (×) to exit

## Development

The project uses:
- .NET 7.0 WPF
- Two DispatcherTimer instances:
  - Reading data from current file every 200ms
  - Checking for new files every 10 seconds
- Extended CSV parsing with all combat log fields
- Combat activity detection (pause > 8 seconds = combat end)
- Combat time calculation excluding pauses > 10 seconds between records
- Automatic combat session history with target-based naming
- Dynamic UI with auto-hide interface and custom styling

## Testing

For testing, create test files in the `%LOCALAPPDATA%\TL\SAVED\COMBATLOGS\` folder with data in the specified format.
