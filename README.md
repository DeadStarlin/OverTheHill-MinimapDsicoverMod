# Map Discovery

MelonLoader mod for **over the hill**, by **DeadStarlin**.

Expands the area uncovered while driving. Uses the game's normal fog removal and POI discovery system. Cabin reveal radii and the update interval stay unchanged.

## Install

1. Install MelonLoader for the IL2CPP version of the game and launch it once.
2. Copy `bin/Release/net6.0/OTHMapDiscovery.dll` into the game's `Mods` folder.
3. Launch the game, then close it to edit the preferences file.

## Settings

In the game's `UserData/MelonPreferences.cfg`, edit the generated category:

```toml
[OTHMapDiscovery]
AreaMultiplier = "x4"
```

Allowed values: `"x1"`, `"x2"`, `"x4"`, `"x10"`, `"x100"`. Default: `"x4"`. Restart the game after editing the file. A compatible preferences editor can also change the setting while playing.

These multiply **area**, not radius:

| Setting | Area | Radius |
| --- | --- | --- |
| x1 | 1x (vanilla) | 1x |
| x2 | 2x | 1.414x |
| x4 | 4x | 2x |
| x10 | 10x | 3.162x |
| x100 | 100x | 10x |

Invalid numeric values fall back to vanilla radius. Newly revealed terrain and discovered POIs are saved normally: switching to x1 or removing the mod does not undo discovery. Back up saves before using it if you want to preserve your exploration progress. x100 may increase fog-update cost and discover many POIs nearby.

## Build and check

```powershell
.\build.ps1
# Optional path overrides:
.\build.ps1 -GameDir "C:\path\to\game" -RuntimeDir "C:\path\to\Microsoft.NETCore.App\6.0.xx"
dotnet run --project tests/MapDiscovery.Tests.csproj -c Release
```

For a POSIX shell (Linux/macOS, or a compatible shell with .NET installed):

```sh
sh build.sh "/path/to/over the hill"
# Optional explicit .NET 6 runtime directory:
sh build.sh "/path/to/over the hill" "/path/to/shared/Microsoft.NETCore.App/6.0.xx"
```

Both scripts build Release from any working directory, check required references, and automatically locate an installed .NET 6 runtime when no runtime path is supplied. The shell script requires the game directory argument; PowerShell defaults to the local game installation.

The project references installed game and MelonLoader assemblies without downloading packages. Defaults are the local game path and .NET 6 runtime; override with `-p:GameDir="C:\path\to\game"` and `-p:RuntimeDir="C:\path\to\Microsoft.NETCore.App\6.0.xx"` if needed. A .NET SDK is required; the checks target .NET 8.

## Compatibility and verification

Built against the supplied game dump and generated MelonLoader IL2CPP assemblies. The native movement coroutine (`0x9196F0`) reads `FogOfWarConfig.playerInnerRadius` and `playerOuterRadius`, calls `UpdateFogOfWarTexture` (`0x918FE0`), then uses the outer radius for POI discovery (`0x919290`). The mod patches fog texture initialization (after config assignment and before movement updates begin) and scales those config fields from their original values, avoiding cumulative scaling when a manager is recreated. Original radii are restored on mod deinitialization.

Compilation, multiplier checks, and generated-wrapper patch target checks are verified. In-game loading, preference serialization, multiplayer behavior, and performance have not been tested. Future game updates that change these types may require rebuilding or updating the patch.

