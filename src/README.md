# Forgeborne Core — Slice 1 Step 1

Pure C# game logic, no UnityEngine dependency, so it can be tested
headlessly.

## What is here

- `Forgeborne.Core/VoxelChunk.cs` — 32^3 chunk, deterministic generation.
- `Forgeborne.Core/Voxel.cs` — block types, gatherability, deterministic hash.
- `Forgeborne.Core/Player.cs` — health 100, shields 85, shields-first damage, inventory (max stack 999).
- `Forgeborne.Core/World.cs` — WorldZone: gather, build (wall: 2 stone, workstation: 10 stone + 5 ore, support rule, no floating builds), enemies (move toward player, attack adjacent for 10), waves, ticks.
- `Forgeborne.Core/SaveSystem.cs` — JSON save / load, chunk as Base64, player, inventory, enemies (including enemy health), seed, tick.

## How to test (headless, no Unity required)

Requires .NET 8 SDK. From this repository root:

```
dotnet run --project tests/Forgeborne.Core.Tests
```

Current runtime checkpoint: **5155 passed, 0 failed**, with a full rebuild
reporting 0 warnings/errors and execution on genuine .NET 8.0.33. The earlier
integration baseline used explicit .NET 9 roll-forward; an isolated verified
.NET 8 runtime has since closed that toolchain gap. See
`docs/courtyard-player-flow-checkpoint.md` for current live Unity evidence.

## Unity integration

Core ships as DLLs under `ForgeborneUnity/Assets/Plugins`; never copy core
source into Assets. The saved Main scene uses one GameManager-owned
Simulation. Open it with Unity 6000.3.27f2 and Active Input Handling = Both.
The core suite remains separate from rendered gameplay and director
acceptance; the current Windows evidence and remaining gaps are in
`docs/unity-main-integration.md`. Networking remains deferred.
