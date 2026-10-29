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

Expected, verified 2026-10-29: **42 passed, 0 failed.**

## What is NOT here yet (Slice 1 Steps 2-4)

- Unity scene, FPS input / movement, voxel meshing / rendering.
- HUD (health, shields, objective, ammo, mini-map).
- Orbital Tactical mode, squads, loot, passive graph, automation tiers.
- Networking (deferred).

Unity integration notes are in `ForgeborneUnity/Assets/Scripts/UnityIntegrationNote.cs`.
That Unity skeleton is NOT compiled in the Linux headless environment;
open `ForgeborneUnity/` in Unity 6000 on Windows to verify it.
